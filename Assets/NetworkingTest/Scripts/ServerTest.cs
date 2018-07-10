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
}

public struct GameObjectSpawnInfo {
    public GameObject obj;
    public string path;
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

    List<int> playerStates;
    List<SerializedBuffer> sendBuffers;
    List<List<GameObjectSpawnInfo>> gameObjectsByOwner; // player controller(connection) id, owned NetGOs.
    Dictionary<int, ReplicatedProperties> synchronizedComponents; // instance id, game object
    List<int> newConnectionList;
    int bufferSize = 1024;
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
        playerStates = new List<int>(defaultMaxPlayerCount);
        sendBuffers = new List<SerializedBuffer>(defaultMaxPlayerCount * 2);
        synchronizedComponents = new Dictionary<int, ReplicatedProperties>();
        gameObjectsByOwner = new List<List<GameObjectSpawnInfo>>();
        newConnectionList = new List<int>();
    }

    private void Start() {
    }

    public void replicateExistingGameObjectsToNewClient(int connId) {
        
        for (int i = 0; i < gameObjectsByOwner.Count; ++i) {
            List<GameObjectSpawnInfo> objList = gameObjectsByOwner[i];
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
            gameObjectsByOwner[idx].Add(gosi);
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
        for (int i = 0; i < playerStates.Count; ++i) {
            if (playerStates[i] == conn_id)
                return i;
        }
        return -1;
    }

    void serializeGameObjectReplication(int[] component_ids, string path, int conn_id = -1)
    {
        if (conn_id == -1) {
            for (int i = 0; i < playerStates.Count; ++i) {
                sendBuffers[i * 2].serializeUShort((ushort)NetOpCodes.SpawnPrefab);
                sendBuffers[i * 2].serializeByte((byte)component_ids.Length);
                // instance id count
                for (int j = 0; j < component_ids.Length; ++j) {
                    sendBuffers[i * 2].serializeInt(component_ids[j]);
                }
                sendBuffers[i * 2].serializeString(path);
                sendBuffers[i * 2].incrementCommandCount();
            }
        }
        else {
            int idx = getIndexByConnectionId(conn_id);
            if (idx >= 0) {
                sendBuffers[idx * 2].serializeUShort((ushort)NetOpCodes.SpawnPrefab);
                sendBuffers[idx * 2].serializeByte((byte)component_ids.Length);
                // instance id count
                for (int j = 0; j < component_ids.Length; ++j) {
                    sendBuffers[idx * 2].serializeInt(component_ids[j]);
                }
                sendBuffers[idx * 2].serializeString(path);
                sendBuffers[idx * 2].incrementCommandCount();
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
                    if (playerStates.Contains(recConnectionId)) {
                        Debug.Log("player " + recConnectionId + " disconnected.");
                        for(int j = 0; j < playerStates.Count; ++j) {
                            if(playerStates[j] == recConnectionId) {
                                playerStates.RemoveAt(j);
                                sendBuffers.RemoveAt(j * 2 + 1);
                                sendBuffers.RemoveAt(j * 2);
                                
                                List<GameObjectSpawnInfo> toRemove = gameObjectsByOwner[j];
                                for(int k = 0; k < toRemove.Count; ++k) {
                                    GameObject.Destroy(toRemove[k].obj);
                                }
                                gameObjectsByOwner.RemoveAt(j);
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
            playerStates.Add(newConnectionList[i]);
            sendBuffers.Add(new SerializedBuffer());
            sendBuffers.Add(new SerializedBuffer());
            gameObjectsByOwner.Add(new List<GameObjectSpawnInfo>());
            replicateExistingGameObjectsToNewClient(newConnectionList[i]);
            spawnReplicatedGameObject(newConnectionList[i], PlayerStatePrefabPath);
        }
        sendAllBuffers();
    }
    public void repVar(int component_id, ushort var_id, int val, byte rep_mode, int conn_id = -1) {
        if (conn_id >= 0) {
            int idx = getIndexByConnectionId(conn_id);
            if (idx >= 0) {
                sendBuffers[idx * 2].repVar(component_id, var_id, val);
            }
        }
        else {
            for (int i = 0; i < playerStates.Count; ++i) {
                sendBuffers[i * 2].repVar(component_id, var_id, val);
            }
        }
    }

    public void repVar(int component_id, ushort var_id, float val, byte rep_mode, int conn_id = -1) {
        if (conn_id >= 0) {
            int idx = getIndexByConnectionId(conn_id);
            if(idx >= 0) {
                sendBuffers[idx * 2].repVar(component_id, var_id, val);
            }
        }
        else {
            for (int i = 0; i < playerStates.Count; ++i) {
                sendBuffers[i * 2].repVar(component_id, var_id, val);
            }
        }
    }
    byte rpcSessionMode;
    List<int> sessionRPCTargetIndices = new List<int>(8);
    public void rpcBegin(int component_id, ushort rpc_id, byte mode, int owner_id) {
        rpcSessionMode = mode;
        sessionRPCTargetIndices.Clear();
        int usingUnreliable = mode & SerializedBuffer.RPCMode_Unreliable;
        if ((rpcSessionMode & SerializedBuffer.RPCMode_ToTarget) > 0) {
            int idx = getIndexByConnectionId(owner_id);
            if (idx >= 0) {
                sessionRPCTargetIndices.Add(idx * 2 + usingUnreliable);
            }
        }
        if ((rpcSessionMode & SerializedBuffer.RPCMode_ExceptTarget) > 0) {
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerStates[i] != owner_id) {
                    sessionRPCTargetIndices.Add(i * 2 + usingUnreliable);
                }
            }
        }

        for (int i = 0; i < sessionRPCTargetIndices.Count; ++i) {
            sendBuffers[sessionRPCTargetIndices[i]].rpcBegin(component_id, rpc_id);
        }
    }
    public void rpcEnd() {
        for (int i = 0; i < sessionRPCTargetIndices.Count; ++i) {
            sendBuffers[sessionRPCTargetIndices[i]].rpcEnd();
        }
    }
    public void rpcAddParam(byte val) {

        for (int i = 0; i < sessionRPCTargetIndices.Count; ++i) {
            sendBuffers[sessionRPCTargetIndices[i]].rpcAddParam(val);
        }
    }
    public void rpcAddParam(int val) {
        for (int i = 0; i < sessionRPCTargetIndices.Count; ++i) {
            sendBuffers[sessionRPCTargetIndices[i]].rpcAddParam(val);
        }
    }
    public void rpcAddParam(float val) {
        for (int i = 0; i < sessionRPCTargetIndices.Count; ++i) {
            sendBuffers[sessionRPCTargetIndices[i]].rpcAddParam(val);
        }
    }
    public void rpcAddParam(Vector3 val) {
        for (int i = 0; i < sessionRPCTargetIndices.Count; ++i) {
            sendBuffers[sessionRPCTargetIndices[i]].rpcAddParam(val);
        }
    }

    void sendAllBuffers()
    {
        byte error = 0;
        // to each
        for (int i = 0; i < playerStates.Count; ++i)
        {
            if (sendBuffers[i * 2].getCommandCount() > 0) {
                sendBuffers[i * 2].seal();
                NetworkTransport.Send(socketId, playerStates[i], reliableCHN, sendBuffers[i * 2].getBuffer(), sendBuffers[i * 2].getOffset(), out error);
                sendBuffers[i * 2].reset();
            }
            if (sendBuffers[i * 2 + 1].getCommandCount() > 0) {
                sendBuffers[i * 2 + 1].seal();
                NetworkTransport.Send(socketId, playerStates[i], unreliableCHN, sendBuffers[i * 2 + 1].getBuffer(), sendBuffers[i * 2 + 1].getOffset(), out error);
                sendBuffers[i * 2 + 1].reset();
            }
        }
    }
}