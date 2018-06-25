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
    Dictionary<int, ReplicatedProperties> ARNetGOs; // instance id, game object
    Dictionary<int, List<RepStates>> NetGOByOwners; // player controller(connection) id, owned NetGOs.

    byte[] sendBuffer;
    int refBufIndex;
    int commandCount;

    // replication
    List<RepItem> repItems;

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
        ARNetGOs = new Dictionary<int, ReplicatedProperties>();
        repItems = new List<RepItem>();

        refBufIndex = 4;
    }

    private void Start() {
    }

    public void SendSocketMessage(int pcid) {
        byte error;
        int bufferSize = 1024;
        byte[] buffer = new byte[bufferSize];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, "Hello Client");

        NetworkTransport.Send(socketId, pcid, reliableCHN, buffer, bufferSize, out error);
    }

    public void spawnReplicatedGameObject(int connId, string path) {
        int[] goid;
        GameObject pcgo = spawnPrefabOnServer(connId, path, out goid);
        spawnNetGameObject2(goid, path);
        repPropertyComponents(pcgo);
    }
    public GameObject spawnPrefabOnServer(int connId, string prefabPath, out int[] comp_ids)
    {
        // first create it on the server.
        UnityEngine.Object netGO = Resources.Load(prefabPath);
        GameObject newNetGO = GameObject.Instantiate(netGO) as GameObject;
        ReplicatedProperties[] repComponents = newNetGO.GetComponents<ReplicatedProperties>();
        comp_ids = new int[repComponents.Length];
        for (int i = 0; i < repComponents.Length; ++i)
        {
            comp_ids[i] = repComponents[i].GetInstanceID();
            ARNetGOs.Add(comp_ids[i], repComponents[i]);
            repComponents[i].owner = connId;
        }
        
        return newNetGO;
    }
    public void repPropertyComponents(GameObject go)
    {
        ReplicatedProperties[] repComponents = go.GetComponents<ReplicatedProperties>();
        for (int i = 0; i < repComponents.Length; ++i)
        {
            repComponents[i].rep_owner();
        }
    }

    byte spawnNetGameObjectOnSingleRemote2(int[] component_ids, string path, int connId)
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
        //NetworkTransport.Send(socketId, connId, reliableCHN, buffer, offset, out error);
        return 0;
    }
    // change this to multicast
    //public void spawnNetGameObject(int goid, string path) {
    //    for (int i = 0; i < playerControllers.Count; ++i) {
    //        spawnNetGameObjectOnSingleRemote(goid, path, playerControllers[i]);
    //    }
    //}

    public void spawnNetGameObject2(int[] goid, string path)
    {
        for (int i = 0; i < playerStates.Count; ++i)
        {
            spawnNetGameObjectOnSingleRemote2(goid, path, playerStates[i]);
        }
    }
    // server calls an RPC on a client.
    public byte callRPCOnClient(GameObject netgo, string methodName, int socketId, int connId, int chnId) {
        byte error = 0;

        //ushort opcode = (ushort)NetOpCodes.RPCFunc;
        //byte[] intBytes = BitConverter.GetBytes(opcode);
        //Array.Copy(intBytes, 0, buffer, 0, intBytes.Length);
        //int GOInstanceID = netgo.GetInstanceID();
        //int offset = intBytes.Length;
        //byte[] instIdBytes = BitConverter.GetBytes(GOInstanceID);
        
        //Array.Copy(instIdBytes, 0, buffer, offset, instIdBytes.Length);
        //offset += instIdBytes.Length;

        //byte[] lengthBytes = BitConverter.GetBytes((ushort)methodName.Length);
        //Array.Copy(lengthBytes, 0, buffer, offset, lengthBytes.Length);
        //offset += lengthBytes.Length;

        //byte[] stringBytes = Encoding.ASCII.GetBytes(methodName);
        //Array.Copy(stringBytes, 0, buffer, offset, stringBytes.Length);
        //offset += stringBytes.Length;

        //NetworkTransport.Send(socketId, connId, chnId, buffer, offset, out error);
        return error;
    }

    // decode rpc from client on server
    public void decodeRPC(byte[] src) {
        //ushort opcode = BitConverter.ToUInt16(src, 0);

        //if (opcode == (ushort)NetOpCodes.RPCFunc) {
        //    int serverGOInstId = BitConverter.ToInt32(src, 2);
        //    if (ARNetGOs.ContainsKey(serverGOInstId)) {
        //        ushort length = BitConverter.ToUInt16(src, 6);
        //        string methodName = Encoding.ASCII.GetString(src, 8, length);
        //        if (string.IsNullOrEmpty(methodName) == false)
        //            ARNetGOs[serverGOInstId].gameObject.SendMessage(methodName);
        //    }
        //}
    }

    private void OnDestroy() {
        //byte error;
        //NetworkTransport.Disconnect(hostId, connectionId, out error);
    }
    public bool sendToClient = false;
    // Update is called once per frame
    GameObject remotePC;
    void Update () {
        if (sendToClient) {
            sendToClient = false;
            for(int i = 0; i < playerStates.Count; ++i) {
                //SendSocketMessage(playerControllers[i]);
                callRPCOnClient(remotePC, "yay", socketId, playerStates[i], reliableCHN);
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
                    Debug.Log(recData.ToString());

                    Stream stream = new MemoryStream(recvBuffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    string message = formatter.Deserialize(stream) as string;
                    Debug.Log("incoming message event received: " + message);

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
    #region ReplicationV2
    //List<byte[]> sepBufferToClients; // separate buffers for each individual client
    //List<int> sepBufferOffsets; // corresponding offsets.
    //byte[] bufferToAll;
    //int offsetToAll;
    //void addRepGameObject(byte[] inoutBuffer, ref int offset, string path, int instanceId) {
    //    //ushort opcode = RepItem.RepGameObject;
    //    //byte[] intBytes = BitConverter.GetBytes(opcode); // 2 opcode
    //    //Array.Copy(intBytes, 0, inoutBuffer, offset, intBytes.Length);
    //    //offset += intBytes.Length;
    //    inoutBuffer[offset] = RepItem.RepGameObject;
    //    offset++;

    //    // instance Id
    //    byte[] instIdBytes = BitConverter.GetBytes(instanceId); // 4 instance id
    //    Array.Copy(instIdBytes, 0, inoutBuffer, offset, instIdBytes.Length);
    //    offset += instIdBytes.Length;

    //    // path string length
    //    byte[] lengthBytes = BitConverter.GetBytes((ushort)path.Length); // 2 path string length
    //    Array.Copy(lengthBytes, 0, inoutBuffer, offset, lengthBytes.Length);
    //    offset += lengthBytes.Length;

    //    // path string
    //    byte[] stringBytes = Encoding.ASCII.GetBytes(path); // ?? path string
    //    Array.Copy(stringBytes, 0, inoutBuffer, offset, stringBytes.Length);
    //    offset += stringBytes.Length;
    //}
    #endregion

    #region ReplicationV3

    public void rflcAddInt(int component_id, ushort offsetId, int intVal)
    {
        // netopcode, ushort
        ClientTest.serializeUShort(sendBuffer, (ushort)NetOpCodes.Replication, ref refBufIndex);

        // variable type, byte
        ClientTest.serializeByte(sendBuffer, RepItem.RepInt, ref refBufIndex);

        // total length, ushort
        ClientTest.serializeUShort(sendBuffer, 10, ref refBufIndex);

        // component id, int
        ClientTest.serializeInt(sendBuffer, component_id, ref refBufIndex);

        // offset id, variable id, ushort
        ClientTest.serializeUShort(sendBuffer, offsetId, ref refBufIndex);

        // int value
        ClientTest.serializeInt(sendBuffer, intVal, ref refBufIndex);
        commandCount++;
    }

    public void rflcAddFloat(int component_id, ushort offsetId, float floatVal)
    {
        // netopcode, ushort
        ClientTest.serializeUShort(sendBuffer, (ushort)NetOpCodes.Replication, ref refBufIndex);

        // variable type, byte
        ClientTest.serializeByte(sendBuffer, RepItem.RepInt, ref refBufIndex);

        // total length, ushort
        ClientTest.serializeUShort(sendBuffer, 10, ref refBufIndex);

        // component id, int
        ClientTest.serializeInt(sendBuffer, component_id, ref refBufIndex);

        // offset id, variable id, ushort
        ClientTest.serializeUShort(sendBuffer, offsetId, ref refBufIndex);

        ClientTest.serializeFloat(sendBuffer, floatVal, ref refBufIndex);

        commandCount++;
    }
    #endregion

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

    #region RPC
    public void invokeServerRPC(string methodName, params object[] paramObjs) {

    }
    #endregion
}
struct RepItem {
    public byte dataType;
    public byte[] value;
    public object ptr;
    public int instanceId;
    public int offset;
    public RepItem(byte type) {
        dataType = type;
        value = null;
        ptr = null;
        instanceId = 0;
        offset = 0;
    }
    public static byte RepInt = 0;
    public static byte RepFloat = 1;
    public static byte RepVec2 = 2;
    public static byte RepVec3 = 3;
    public static byte RepString = 4;
    public static byte RepIntArray = 5;
    public static byte RepFloatArray = 6;
    public static byte RepGameObject = 255;
};