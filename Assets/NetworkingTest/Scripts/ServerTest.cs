using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
enum NetOpCodes : ushort {
    Reserved, // unused
    ServerTerminate,
    SpawnPrefab, // string, replicate a prefab from server to clients.
    RPCFunc, // rpc
    Replication, // replication of variable
    ObjectLinking,
}

public struct GameObjectSpawnInfo {
    public GameObject obj;
    public string path;
}
public class PlayerOwnedInfo {
    public int connectionId;
    public SerializedBuffer[] sendBuffers;
    public List<GameObjectSpawnInfo> gameObjectsOwned;
    public PlayerOwnedInfo(int _connId) {
        connectionId = _connId;
        if (connectionId >= 0) {
            sendBuffers = new SerializedBuffer[2];
            sendBuffers[0] = new SerializedBuffer();
            sendBuffers[1] = new SerializedBuffer();
        }
        else {
            sendBuffers = null;
        }
        gameObjectsOwned = new List<GameObjectSpawnInfo>();
    }

}
public class ServerTest : MonoBehaviour {
    // Use this for initialization
    public static ServerTest self;
    int serverPort = 427;
    int socketId;
    int reliableCHN;
    int unreliableCHN;
    
    public string PlayerStatePrefabPath;

    byte[] recvBuffer;

    List<PlayerOwnedInfo> playerOwned;
    Dictionary<int, ReplicatedProperties> synchronizedComponents; // instance id, game object
    List<int> newConnectionList;
    int bufferSize = 1024;
    byte rpcSessionMode;
    List<int> sessionRPCTargetIndices = new List<int>(8);
    void Awake () {
        self = this;
        Application.runInBackground = true;
        NetworkTransport.Init();
        GlobalConfig gConfig = new GlobalConfig();
        gConfig.MaxPacketSize = 512;
        NetworkTransport.Init(gConfig);

        ConnectionConfig config = new ConnectionConfig();
        reliableCHN = config.AddChannel(QosType.AllCostDelivery);
        unreliableCHN = config.AddChannel(QosType.StateUpdate);

        HostTopology topology = new HostTopology(config, 10); // max connections
        //socketId = NetworkTransport.AddHost(topology, serverPort);
        socketId = NetworkTransport.AddHostWithSimulator(topology, 30, 200, serverPort);
        Debug.Log("Socket Open. SocketId is: " + socketId);
        recvBuffer = new byte[bufferSize];

        int defaultMaxPlayerCount = 8;
        playerOwned = new List<PlayerOwnedInfo>(defaultMaxPlayerCount);
        //playerStates = new List<int>(defaultMaxPlayerCount);
        //sendBuffers = new List<SerializedBuffer>(defaultMaxPlayerCount * 2);
        //gameObjectsByOwner = new List<List<GameObjectSpawnInfo>>();
        synchronizedComponents = new Dictionary<int, ReplicatedProperties>();
        
        newConnectionList = new List<int>();
    }
    const int ServerPSId = -1;
    private void Start() {
        GameObject go = createServerGameObject(PlayerStatePrefabPath);
        PlayerState ps = go.GetComponent<PlayerState>();
        ps.isHost = 1; 

        List<INeutralObject> npcs = PendingNetworkObjects.self.getNPCs();
        for(int i = 0; i < npcs.Count; ++i) {
            npcs[i].onServerInitialized();
        }
    }
    public GameObject createServerGameObject(string path) {
        int[] goid;
        GameObject pcgo = spawnPrefabOnServer(ServerPSId, path, out goid);

        ReplicatedProperties[] repComponents = pcgo.GetComponents<ReplicatedProperties>();
        for (int i = 0; i < repComponents.Length; ++i) {
            repComponents[i].isHost = 1;
            repComponents[i].role = GameObjectRoles.Authority;
            repComponents[i].initialReplicationComplete();
        }
        playerOwned.Add(new PlayerOwnedInfo(ServerPSId));
        int idx = getIndexByConnectionId(ServerPSId);
        if (idx >= 0) {
            GameObjectSpawnInfo gosi = new GameObjectSpawnInfo();
            gosi.path = path;
            gosi.obj = pcgo;
            playerOwned[idx].gameObjectsOwned.Add(gosi);
        }
        return pcgo;
    }
    public void addNPCsToSyncList(ReplicatedProperties rep) {
        synchronizedComponents.Add(rep.server_id, rep);
    }

    public void replicateExistingGameObjectsToNewClient(int connId) {
        
        for (int i = 0; i < playerOwned.Count; ++i) {
            List<GameObjectSpawnInfo> objList = playerOwned[i].gameObjectsOwned;
            for(int j = 0; j < objList.Count; ++j) {
                ReplicatedProperties[] repComponents = objList[j].obj.GetComponents<ReplicatedProperties>();
                int[] comp_ids = new int[repComponents.Length];
                for (int k = 0; k < repComponents.Length; ++k) {
                    comp_ids[k] = repComponents[k].server_id;
                }
                serializeGameObjectReplication(comp_ids, objList[j].path, connId);

                for (int k = 0; k < repComponents.Length; ++k) {
                    repComponents[k].replicateAllStates(0, connId);
                    repComponents[k].clientSetRole(connId);
                }
            }
        }
    }
    // connId, owner id.
    public GameObject spawnReplicatedGameObject(int connId, string path) {
        int[] goid;
        GameObject pcgo = spawnPrefabOnServer(connId, path, out goid);

        // replicate this to all clients.
        serializeGameObjectReplication(goid, path, -1);

        ReplicatedProperties[] repComponents = pcgo.GetComponents<ReplicatedProperties>();
        for (int i = 0; i < repComponents.Length; ++i) {
            repComponents[i].replicateAllStates(SerializedBuffer.RPCMode_ToTarget | SerializedBuffer.RPCMode_ExceptTarget);
            repComponents[i].clientSetRole();
        }

        int idx = getIndexByConnectionId(connId);
        if(idx >= 0) {
            GameObjectSpawnInfo gosi = new GameObjectSpawnInfo();
            gosi.path = path;
            gosi.obj = pcgo;
            playerOwned[idx].gameObjectsOwned.Add(gosi);
        }
        
        return pcgo;
    }
    GameObject spawnPrefabOnServer(int connId, string prefabPath, out int[] comp_ids)
    {
        // first create it on the server.
        UnityEngine.Object netGO = Resources.Load(prefabPath);
        GameObject newNetGO = GameObject.Instantiate(netGO) as GameObject;
        ReplicatedProperties[] repComponents = newNetGO.GetComponents<ReplicatedProperties>();
        comp_ids = new int[repComponents.Length];
        for (int i = 0; i < repComponents.Length; ++i)
        {
            comp_ids[i] = repComponents[i].server_id;
            synchronizedComponents.Add(comp_ids[i], repComponents[i]);
            repComponents[i].owner = connId;
        }
        
        return newNetGO;
    }
    int getIndexByConnectionId(int conn_id) {
        for (int i = 0; i < playerOwned.Count; ++i) {
            if (playerOwned[i].connectionId == conn_id)
                return i;
        }
        return -1;
    }

    void serializeGameObjectReplication(int[] component_ids, string path, int conn_id = -1)
    {
        if (conn_id == -1) {
            for (int i = 0; i < playerOwned.Count; ++i) {
                if (playerOwned[i].connectionId == ServerPSId) continue;

                playerOwned[i].sendBuffers[0].serializeUShort((ushort)NetOpCodes.SpawnPrefab);
                playerOwned[i].sendBuffers[0].serializeByte((byte)component_ids.Length);
                // instance id count
                for (int j = 0; j < component_ids.Length; ++j) {
                    playerOwned[i].sendBuffers[0].serializeInt(component_ids[j]);
                }
                playerOwned[i].sendBuffers[0].serializeString(path);
                playerOwned[i].sendBuffers[0].incrementCommandCount();
            }
        }
        else {
            int idx = getIndexByConnectionId(conn_id);
            if (idx >= 0) {
                playerOwned[idx].sendBuffers[0].serializeUShort((ushort)NetOpCodes.SpawnPrefab);
                playerOwned[idx].sendBuffers[0].serializeByte((byte)component_ids.Length);
                // instance id count
                for (int j = 0; j < component_ids.Length; ++j) {
                    playerOwned[idx].sendBuffers[0].serializeInt(component_ids[j]);
                }
                playerOwned[idx].sendBuffers[0].serializeString(path);
                playerOwned[idx].sendBuffers[0].incrementCommandCount();
            }
        }

    }

    private void OnDestroy() {
        //byte error;
        //NetworkTransport.Disconnect(hostId, connectionId, out error);
    }
    void FixedUpdate() {
        bool loop = true;

        int recHostId;
        int recConnectionId;
        int channelId;
        
        int dataSize;
        byte error;
        newConnectionList.Clear();
        for (int i = 0; i < 10 && loop; ++i) {
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out recConnectionId, out channelId, recvBuffer, bufferSize, out dataSize, out error);
            if(dataSize >= bufferSize) {
                Debug.LogWarning("the data received exceeds the buffer size!");
            }
            //if (channelId == unreliableCHN) {
            //    Debug.Log("unreliable: " + dataSize);
            //}
            switch (recData) {
                case NetworkEventType.Nothing:         //1
                    loop = false;
                    break;
                case NetworkEventType.ConnectEvent:    //2
                    Debug.Log("socket id " + recHostId + ", conn " + recConnectionId + ", channel " + channelId);
                    newConnectionList.Add(recConnectionId);
                    break;
                case NetworkEventType.DataEvent:       //3
                    ClientTest.decodeRawData(recvBuffer, synchronizedComponents);
                    break;
                case NetworkEventType.DisconnectEvent: //4
                    int idx = getIndexByConnectionId(recConnectionId);
                    if (idx >= 0) {
                        Debug.Log("player " + recConnectionId + " disconnected.");
                        for(int j = 0; j < playerOwned.Count; ++j) {
                            if(playerOwned[j].connectionId == recConnectionId) {
                                //playerStates.RemoveAt(j);
                                //sendBuffers.RemoveAt(j * 2 + 1);
                                //sendBuffers.RemoveAt(j * 2);
                                
                                List<GameObjectSpawnInfo> toRemove = playerOwned[j].gameObjectsOwned;
                                for(int k = 0; k < toRemove.Count; ++k) {
                                    GameObject.Destroy(toRemove[k].obj);
                                }
                                playerOwned.RemoveAt(j);
                                break;
                            }
                        }
                    }
                    Debug.Log(recData.ToString());
                    break;
            }
        }
        for(int i = 0; i < newConnectionList.Count; ++i) {
            // create player state
            playerOwned.Add(new PlayerOwnedInfo(newConnectionList[i]));
            replicateExistingGameObjectsToNewClient(newConnectionList[i]);
            spawnReplicatedGameObject(newConnectionList[i], PlayerStatePrefabPath);
        }
        sendAllBuffers();
    }
    public void repVar(int component_id, ushort var_id, int val, byte rep_mode, int conn_id = -1) {
        if ((rep_mode & SerializedBuffer.RPCMode_ToTarget) > 0 &&
            (rep_mode & SerializedBuffer.RPCMode_ExceptTarget) > 0) {

            for (int i = 0; i < playerOwned.Count; ++i) {
                if (playerOwned[i].connectionId == ServerPSId) continue;
                playerOwned[i].sendBuffers[0].repVar(component_id, var_id, val);
            }
        }
        else {
            int idx = getIndexByConnectionId(conn_id);
            if (idx >= 0) {
                if ((rep_mode & SerializedBuffer.RPCMode_ToTarget) > 0) {
                    playerOwned[idx].sendBuffers[0].repVar(component_id, var_id, val);
                }
                else {
                    if ((rep_mode & SerializedBuffer.RPCMode_ExceptTarget) > 0) {
                        for (int i = 0; i < playerOwned.Count; ++i) {
                            if (playerOwned[i].connectionId == ServerPSId || playerOwned[i].connectionId == conn_id) continue;

                            playerOwned[i].sendBuffers[0].repVar(component_id, var_id, val);
                        }
                    }
                }
            }
        }
    }
    public void repVar(int component_id, ushort var_id, float val, byte rep_mode, int conn_id = -1) {
        if ((rep_mode & SerializedBuffer.RPCMode_ToTarget) > 0 &&
            (rep_mode & SerializedBuffer.RPCMode_ExceptTarget) > 0) {

            for (int i = 0; i < playerOwned.Count; ++i) {
                if (playerOwned[i].connectionId == ServerPSId) continue;
                playerOwned[i].sendBuffers[0].repVar(component_id, var_id, val);
            }
        }
        else {
            int idx = getIndexByConnectionId(conn_id);
            if (idx >= 0) {
                if ((rep_mode & SerializedBuffer.RPCMode_ToTarget) > 0) {
                    playerOwned[idx].sendBuffers[0].repVar(component_id, var_id, val);
                }
                else {
                    if ((rep_mode & SerializedBuffer.RPCMode_ExceptTarget) > 0) {
                        for (int i = 0; i < playerOwned.Count; ++i) {
                            if (playerOwned[i].connectionId == ServerPSId || playerOwned[i].connectionId == conn_id) continue;

                            playerOwned[i].sendBuffers[0].repVar(component_id, var_id, val);
                        }
                    }
                }
            }
        }
    }

    public void repVar(int component_id, ushort var_id, byte val, byte rep_mode, int conn_id = -1) {
        if ((rep_mode & SerializedBuffer.RPCMode_ToTarget) > 0 &&
            (rep_mode & SerializedBuffer.RPCMode_ExceptTarget) > 0) {

            for (int i = 0; i < playerOwned.Count; ++i) {
                if (playerOwned[i].connectionId == ServerPSId) continue;
                playerOwned[i].sendBuffers[0].repVar(component_id, var_id, val);
            }
        }
        else {
            int idx = getIndexByConnectionId(conn_id);
            if (idx >= 0) {
                if ((rep_mode & SerializedBuffer.RPCMode_ToTarget) > 0) {
                    playerOwned[idx].sendBuffers[0].repVar(component_id, var_id, val);
                }
                else {
                    if ((rep_mode & SerializedBuffer.RPCMode_ExceptTarget) > 0) {
                        for (int i = 0; i < playerOwned.Count; ++i) {
                            if (playerOwned[i].connectionId == ServerPSId || playerOwned[i].connectionId == conn_id) continue;

                            playerOwned[i].sendBuffers[0].repVar(component_id, var_id, val);
                        }
                    }
                }
            }
        }
    }
    
    public void rpcBegin(int component_id, ushort rpc_id, byte mode, int owner_id) {
        rpcSessionMode = mode;
        sessionRPCTargetIndices.Clear();
        int usingUnreliable = ((mode & SerializedBuffer.RPCMode_Unreliable) > 0) ? 1 : 0;
        if ((rpcSessionMode & SerializedBuffer.RPCMode_ToTarget) > 0) {
            int idx = getIndexByConnectionId(owner_id);
            if (idx >= 0 && playerOwned[idx].connectionId != ServerPSId) {
                sessionRPCTargetIndices.Add(idx * 2 + usingUnreliable);
            }
        }
        if ((rpcSessionMode & SerializedBuffer.RPCMode_ExceptTarget) > 0) {
            for (int i = 0; i < playerOwned.Count; ++i) {
                if (playerOwned[i].connectionId != owner_id && playerOwned[i].connectionId != ServerPSId) {
                    sessionRPCTargetIndices.Add(i * 2 + usingUnreliable);
                }
            }
        }

        for (int i = 0; i < sessionRPCTargetIndices.Count; ++i) {
            int idx0 = sessionRPCTargetIndices[i] >> 1;
            int idx1 = sessionRPCTargetIndices[i] % 2;
            playerOwned[idx0].sendBuffers[idx1].rpcBegin(component_id, rpc_id);
        }
    }
    public void rpcEnd() {
        for (int i = 0; i < sessionRPCTargetIndices.Count; ++i) {
            int idx0 = sessionRPCTargetIndices[i] >> 1;
            int idx1 = sessionRPCTargetIndices[i] % 2;
            playerOwned[idx0].sendBuffers[idx1].rpcEnd();
        }
    }
    public void rpcAddParam(byte val) {

        for (int i = 0; i < sessionRPCTargetIndices.Count; ++i) {
            int idx0 = sessionRPCTargetIndices[i] >> 1;
            int idx1 = sessionRPCTargetIndices[i] % 2;
            playerOwned[idx0].sendBuffers[idx1].rpcAddParam(val);
        }
    }
    public void rpcAddParam(int val) {
        for (int i = 0; i < sessionRPCTargetIndices.Count; ++i) {
            int idx0 = sessionRPCTargetIndices[i] >> 1;
            int idx1 = sessionRPCTargetIndices[i] % 2;
            playerOwned[idx0].sendBuffers[idx1].rpcAddParam(val);
        }
    }
    public void rpcAddParam(float val) {
        for (int i = 0; i < sessionRPCTargetIndices.Count; ++i) {
            int idx0 = sessionRPCTargetIndices[i] >> 1;
            int idx1 = sessionRPCTargetIndices[i] % 2;
            playerOwned[idx0].sendBuffers[idx1].rpcAddParam(val);
        }
    }
    public void rpcAddParam(Vector3 val) {
        for (int i = 0; i < sessionRPCTargetIndices.Count; ++i) {
            int idx0 = sessionRPCTargetIndices[i] >> 1;
            int idx1 = sessionRPCTargetIndices[i] % 2;
            playerOwned[idx0].sendBuffers[idx1].rpcAddParam(val);
        }
    }

    void sendAllBuffers()
    {
        byte error = 0;
        // to each
        for (int i = 0; i < playerOwned.Count; ++i)
        {
            PlayerOwnedInfo poi = playerOwned[i];
            if (poi.connectionId == ServerPSId) continue;
            SerializedBuffer buffer_reliable = poi.sendBuffers[0];
            SerializedBuffer buffer_unreliable = poi.sendBuffers[1];
            if (buffer_reliable.getCommandCount() > 0) {
                buffer_reliable.seal();
                NetworkTransport.Send(socketId, poi.connectionId, reliableCHN, buffer_reliable.getBuffer(), buffer_reliable.getOffset(), out error);
                buffer_reliable.reset();
            }
            if (buffer_unreliable.getCommandCount() > 0) {
                buffer_unreliable.seal();
                NetworkTransport.Send(socketId, poi.connectionId, unreliableCHN, buffer_unreliable.getBuffer(), buffer_unreliable.getOffset(), out error);
                buffer_unreliable.reset();
            }
        }
    }
}