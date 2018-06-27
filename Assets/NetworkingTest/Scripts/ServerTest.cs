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
    Replication, // replication
}
public struct RepStates {
    public ReplicatedProperties repGO;
    public List<int> replicatedToIds;
    public RepStates(ReplicatedProperties _go) {
        repGO = _go;
        replicatedToIds = new List<int>(8);
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

    List<int> playerStates;
    List<byte> playerActiveStates;
    List<SerializedBuffer> sendBuffers;
    SerializedBuffer toAllBuffer;
    //Dictionary<int, List<RepStates>> NetGOByOwners; // player controller(connection) id, owned NetGOs.

    Dictionary<int, ReplicatedProperties> synchronizedComponents; // instance id, game object
    void Awake () {
        self = this;
        Application.runInBackground = true;
        NetworkTransport.Init();
        GlobalConfig gConfig = new GlobalConfig();
        gConfig.MaxPacketSize = 500;
        NetworkTransport.Init(gConfig);

        ConnectionConfig config = new ConnectionConfig();
        reliableCHN = config.AddChannel(QosType.Reliable);
        unreliableCHN = config.AddChannel(QosType.Unreliable);

        HostTopology topology = new HostTopology(config, 10); // max connections
        socketId = NetworkTransport.AddHost(topology, serverPort);
        Debug.Log("Socket Open. SocketId is: " + socketId);
        recvBuffer = new byte[1024];

        int defaultMaxPlayerCount = 8;
        playerStates = new List<int>(defaultMaxPlayerCount);
        
        playerActiveStates = new List<byte>(defaultMaxPlayerCount);
        sendBuffers = new List<SerializedBuffer>(defaultMaxPlayerCount);

        for(int i = 0; i < defaultMaxPlayerCount; ++i) {
            sendBuffers.Add(new SerializedBuffer());
            playerStates.Add(0);
            playerActiveStates.Add(0);
        }
        toAllBuffer = new SerializedBuffer();

        synchronizedComponents = new Dictionary<int, ReplicatedProperties>();
    }
    int getNewPlayerIndex() {
        for(int i = 0; i < playerActiveStates.Count; ++i) {
            if(playerActiveStates[i] == 0) {
                playerActiveStates[i] = 1;
                return i;
            }
        }
        playerActiveStates.Add(1);
        return playerActiveStates.Count - 1;
    }

    private void Start() {
    }

    public void spawnReplicatedGameObject(int connId, string path) {
        int[] goid;
        GameObject pcgo = spawnPrefabOnServer(connId, path, out goid);
        spawnNetGameObject(goid, path);
        repPropertyComponents(pcgo);
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
            comp_ids[i] = repComponents[i].GetInstanceID();
            synchronizedComponents.Add(comp_ids[i], repComponents[i]);
            repComponents[i].owner = connId;
        }
        
        return newNetGO;
    }
    void repPropertyComponents(GameObject go)
    {
        ReplicatedProperties[] repComponents = go.GetComponents<ReplicatedProperties>();
        for (int i = 0; i < repComponents.Length; ++i)
        {
            repComponents[i].rep_owner();
        }
    }

    public void spawnNetGameObject(int[] component_ids, string path)
    {
        for (int i = 0; i < playerStates.Count; ++i)
        {
            //spawnNetGameObjectOnSingleRemote2(goid, path, playerStates[i]);

            // 2    1   4x?   2     ?
            toAllBuffer.serializeUShort((ushort)NetOpCodes.SpawnPrefab);
            toAllBuffer.serializeByte((byte)component_ids.Length);
            // instance id count
            for (int j = 0; j < component_ids.Length; ++j) {
                toAllBuffer.serializeInt(component_ids[j]);
            }
            toAllBuffer.serializeString(path);
        }
    }

    private void OnDestroy() {
        //byte error;
        //NetworkTransport.Disconnect(hostId, connectionId, out error);
    }
    //public bool sendToClient = false;
    void FixedUpdate() {
        //if (sendToClient) {
        //    sendToClient = false;
        //    for(int i = 0; i < playerStates.Count; ++i) {
        //    }
        //}
        bool loop = true;

        int recHostId;
        int recConnectionId;
        int channelId;
        
        int bufferSize = 1024;
        int dataSize;
        byte error;
        for (int i = 0; i < 10 && loop; ++i) {
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out recConnectionId, out channelId, recvBuffer, bufferSize, out dataSize, out error);
            switch (recData) {
                case NetworkEventType.Nothing:         //1
                    loop = false;
                    break;
                case NetworkEventType.ConnectEvent:    //2
                    // create player controller
                    Debug.Log("socket id " + recHostId + ", conn " + recConnectionId + ", channel " + channelId);
                    int newPlayerIndex = getNewPlayerIndex();
                    if (newPlayerIndex >= 0) {
                        playerStates[newPlayerIndex] = recConnectionId;


                        spawnReplicatedGameObject(recConnectionId, PlayerStatePrefabPath);
                    }
                    else {
                        Debug.Log("new PlayerState failed to create!");
                    }
                    break;
                case NetworkEventType.DataEvent:       //3
                    ClientTest.decodeRawData(recvBuffer, synchronizedComponents);
                    break;
                case NetworkEventType.DisconnectEvent: //4
                    if (playerStates.Contains(recConnectionId)) {
                        Debug.Log("player " + recConnectionId + " disconnected.");
                        playerStates.Remove(recConnectionId);
                    }
                    Debug.Log(recData.ToString());
                    break;
            }
        }
        replicateToClients();
    }
    public void repVar(int component_id, ushort var_id, int val, byte rep_mode) {
        toAllBuffer.repInt(component_id, var_id, val);
    }
    public void repVar(int component_id, ushort var_id, float val, byte rep_mode) {
        toAllBuffer.repFloat(component_id, var_id, val);
    }

    public void rpcBegin(int component_id, ushort rpc_id, byte mode) {
        toAllBuffer.rpcBegin(component_id, rpc_id, mode);
    }
    public void rpcEnd() {
        toAllBuffer.rpcEnd();
    }
    public void rpcAddParam(byte val) {
        toAllBuffer.rpcAddParam(val);
    }
    public void rpcAddParam(int val) {
        toAllBuffer.rpcAddParam(val);
    }
    public void rpcAddParam(float val) {
        toAllBuffer.rpcAddParam(val);
    }

    void replicateToClients()
    {
        byte error = 0;
        // to each
        for (int i = 0; i < playerStates.Count; ++i)
        {
            if (playerActiveStates[i] == 1 && sendBuffers[i].getCommandCount() > 0) {
                sendBuffers[i].seal();
                NetworkTransport.Send(socketId, playerStates[i], reliableCHN, sendBuffers[i].getBuffer(), sendBuffers[i].getOffset(), out error);
                sendBuffers[i].reset();
            }
        }
        // to all
        if (toAllBuffer.getCommandCount() > 0) {
            toAllBuffer.seal();
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerActiveStates[i] == 1) {
                    // change this to multicast?
                    NetworkTransport.Send(socketId, playerStates[i], reliableCHN, toAllBuffer.getBuffer(), toAllBuffer.getOffset(), out error);
                }
            }
            toAllBuffer.reset();
        }
    }
}