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
    byte[] recvBuffer;
    List<int> playerStates;
    public string PlayerStatePrefabPath;
    Dictionary<int, ReplicatedProperties> synchronizedComponents; // instance id, game object
    //Dictionary<int, List<RepStates>> NetGOByOwners; // player controller(connection) id, owned NetGOs.

    byte[] sendBuffer;
    int refBufIndex;
    int commandCount;

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
        sendBuffer = new byte[1024];
        recvBuffer = new byte[1024];
        playerStates = new List<int>(8);
        synchronizedComponents = new Dictionary<int, ReplicatedProperties>();

        refBufIndex = 4;
    }

    private void Start() {
    }

    public void spawnReplicatedGameObject(int connId, string path) {
        int[] goid;
        GameObject pcgo = spawnPrefabOnServer(connId, path, out goid);
        spawnNetGameObject2(goid, path);
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

    void spawnNetGameObjectOnSingleRemote2(int[] component_ids, string path, int connId)
    {
        int offset = refBufIndex;
        // 2    1   4x?   2     ?
        ClientTest.serializeUShort(sendBuffer, (ushort)NetOpCodes.SpawnPrefab, ref offset);
        ClientTest.serializeByte(sendBuffer, (byte)component_ids.Length, ref offset);
        // instance id count
        for(int i = 0; i < component_ids.Length; ++i)
        {
            ClientTest.serializeInt(sendBuffer, component_ids[i], ref offset);
        }

        ClientTest.serializeString(sendBuffer, path, ref offset);

        commandCount++;
        refBufIndex = offset;
    }

    public void spawnNetGameObject2(int[] goid, string path)
    {
        for (int i = 0; i < playerStates.Count; ++i)
        {
            spawnNetGameObjectOnSingleRemote2(goid, path, playerStates[i]);
        }
    }

    private void OnDestroy() {
        //byte error;
        //NetworkTransport.Disconnect(hostId, connectionId, out error);
    }
    public bool sendToClient = false;
    void Update () {
        if (sendToClient) {
            sendToClient = false;
            for(int i = 0; i < playerStates.Count; ++i) {
                //SendSocketMessage(playerControllers[i]);
                //callRPCOnClient(remotePC, "yay", socketId, playerStates[i], reliableCHN);
            }
        }
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
                    playerStates.Add(recConnectionId);
                    
                    spawnReplicatedGameObject(recConnectionId, PlayerStatePrefabPath);
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
 
    void replicateToClients()
    {
        if (commandCount == 0) return;
        int tmpOffset = 0;
        ClientTest.serializeInt(sendBuffer, commandCount, ref tmpOffset);

        byte error = 0;
        for (int i = 0; i < playerStates.Count; ++i)
        {
            NetworkTransport.Send(socketId, playerStates[i], reliableCHN, sendBuffer, refBufIndex, out error);
        }
        commandCount = 0;
        refBufIndex = 4;
    }
}