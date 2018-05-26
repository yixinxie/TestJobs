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
    SpawnPrefab, // string
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
    byte[] recBuffer;
    List<int> playerControllers;
    public string PlayerControllerPrefabPath;
    Dictionary<int, ReplicatedProperties> ARNetGOs; // instance id, game object
    Dictionary<int, List<RepStates>> NetGOByOwners; // player controller(connection) id, owned NetGOs.

    byte[] buffer;

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
        buffer = new byte[1024];
        recBuffer = new byte[1024];
        playerControllers = new List<int>(8);
        ARNetGOs = new Dictionary<int, ReplicatedProperties>();
        repItems = new List<RepItem>();
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
    
    public byte createPlayerController(int connId, out int goid) {
        // first create it on the server.
        UnityEngine.Object NetGOObj = Resources.Load(PlayerControllerPrefabPath);
        GameObject newNetGO = GameObject.Instantiate(NetGOObj) as GameObject;
        ReplicatedProperties_PlayerController repProps = newNetGO.GetComponent<ReplicatedProperties>() as ReplicatedProperties_PlayerController;
        //ReplicatedProperties repProps = newNetGO.GetComponent<ReplicatedProperties>() as ReplicatedProperties;
        goid = repProps.GetInstanceID();
        RepStates newRepState = new RepStates(repProps);

        ARNetGOs.Add(goid, repProps);
        remotePC = newNetGO;    //???
        repProps.owner = connId;
        repProps.rep_owner();
        //return spawnNetGameObjectOnSingleRemote(goid, PlayerControllerPrefabPath, connId);
        return 0;
    }

    byte spawnNetGameObjectOnSingleRemote(int goid, string path, int connId) {
        byte error;

        ushort opcode = (ushort)NetOpCodes.SpawnPrefab;
        byte[] intBytes = BitConverter.GetBytes(opcode);
        Array.Copy(intBytes, 0, buffer, 0, intBytes.Length);
        int offset = intBytes.Length;

        // instance Id
        byte[] instIdBytes = BitConverter.GetBytes(goid);
        Array.Copy(instIdBytes, 0, buffer, offset, instIdBytes.Length);
        offset += instIdBytes.Length;

        // path string length
        byte[] lengthBytes = BitConverter.GetBytes((ushort)path.Length);
        Array.Copy(lengthBytes, 0, buffer, offset, lengthBytes.Length);
        offset += lengthBytes.Length;

        // path string
        byte[] stringBytes = Encoding.ASCII.GetBytes(path);
        Array.Copy(stringBytes, 0, buffer, offset, stringBytes.Length);
        offset += stringBytes.Length;

        NetworkTransport.Send(socketId, connId, reliableCHN, buffer, offset, out error);
        return error;
    }
    // change this to multicast
    public void spawnNetGameObject(int goid, string path) {
        for (int i = 0; i < playerControllers.Count; ++i) {
            spawnNetGameObjectOnSingleRemote(goid, path, playerControllers[i]);
        }
    }
    // server calls an RPC on a client.
    public byte callRPCOnClient(GameObject netgo, string methodName, int socketId, int connId, int chnId) {
        byte error;

        ushort opcode = (ushort)NetOpCodes.RPCFunc;
        byte[] intBytes = BitConverter.GetBytes(opcode);
        Array.Copy(intBytes, 0, buffer, 0, intBytes.Length);
        int GOInstanceID = netgo.GetInstanceID();
        int offset = intBytes.Length;
        byte[] instIdBytes = BitConverter.GetBytes(GOInstanceID);
        
        Array.Copy(instIdBytes, 0, buffer, offset, instIdBytes.Length);
        offset += instIdBytes.Length;

        byte[] lengthBytes = BitConverter.GetBytes((ushort)methodName.Length);
        Array.Copy(lengthBytes, 0, buffer, offset, lengthBytes.Length);
        offset += lengthBytes.Length;

        byte[] stringBytes = Encoding.ASCII.GetBytes(methodName);
        Array.Copy(stringBytes, 0, buffer, offset, stringBytes.Length);
        offset += stringBytes.Length;

        NetworkTransport.Send(socketId, connId, chnId, buffer, offset, out error);
        return error;
    }

    // decode rpc from client on server
    public void decodeRPC(byte[] src) {
        ushort opcode = BitConverter.ToUInt16(src, 0);

        if (opcode == (ushort)NetOpCodes.RPCFunc) {
            int serverGOInstId = BitConverter.ToInt32(src, 2);
            if (ARNetGOs.ContainsKey(serverGOInstId)) {
                ushort length = BitConverter.ToUInt16(src, 6);
                string methodName = Encoding.ASCII.GetString(src, 8, length);
                if (string.IsNullOrEmpty(methodName) == false)
                    ARNetGOs[serverGOInstId].gameObject.SendMessage(methodName);
            }
        }
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
            for(int i = 0; i < playerControllers.Count; ++i) {
                //SendSocketMessage(playerControllers[i]);
                callRPCOnClient(remotePC, "yay", socketId, playerControllers[i], reliableCHN);
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
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out recConnectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
            switch (recData) {
                case NetworkEventType.Nothing:         //1
                    loop = false;
                    break;
                case NetworkEventType.ConnectEvent:    //2
                    // create player controller
                    Debug.Log("socket id " + recHostId + ", conn " + recConnectionId + ", channel " + channelId);
                    playerControllers.Add(recConnectionId);
                    int goid;
                    createPlayerController(recConnectionId, out goid);
                    spawnNetGameObject(goid, PlayerControllerPrefabPath);
                    break;
                case NetworkEventType.DataEvent:       //3
                    Debug.Log(recData.ToString());

                    Stream stream = new MemoryStream(recBuffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    string message = formatter.Deserialize(stream) as string;
                    Debug.Log("incoming message event received: " + message);

                    break;
                case NetworkEventType.DisconnectEvent: //4
                    if (playerControllers.Contains(recConnectionId)) {
                        Debug.Log("player " + recConnectionId + " disconnected.");
                        playerControllers.Remove(recConnectionId);
                    }
                    Debug.Log(recData.ToString());
                    break;
            }
        }

        replicateToClients();
    }
    #region ReplicationV2
    List<byte[]> sepBufferToClients; // separate buffers for each individual client
    List<int> sepBufferOffsets; // corresponding offsets.
    byte[] bufferToAll;
    int offsetToAll;
    void addRepGameObject(byte[] inoutBuffer, ref int offset, string path, int instanceId) {
        //ushort opcode = RepItem.RepGameObject;
        //byte[] intBytes = BitConverter.GetBytes(opcode); // 2 opcode
        //Array.Copy(intBytes, 0, inoutBuffer, offset, intBytes.Length);
        //offset += intBytes.Length;
        inoutBuffer[offset] = RepItem.RepGameObject;
        offset++;

        // instance Id
        byte[] instIdBytes = BitConverter.GetBytes(instanceId); // 4 instance id
        Array.Copy(instIdBytes, 0, inoutBuffer, offset, instIdBytes.Length);
        offset += instIdBytes.Length;

        // path string length
        byte[] lengthBytes = BitConverter.GetBytes((ushort)path.Length); // 2 path string length
        Array.Copy(lengthBytes, 0, inoutBuffer, offset, lengthBytes.Length);
        offset += lengthBytes.Length;

        // path string
        byte[] stringBytes = Encoding.ASCII.GetBytes(path); // ?? path string
        Array.Copy(stringBytes, 0, inoutBuffer, offset, stringBytes.Length);
        offset += stringBytes.Length;
    }
#endregion

    #region Replication
    public void addRepItem(int goid, int offsetId, int intVal) {
        RepItem newItem = new RepItem(RepItem.RepInt);
        newItem.instanceId = goid;
        newItem.offset = offsetId;
        newItem.value = BitConverter.GetBytes(intVal);
        newItem.ptr = intVal;
        repItems.Add(newItem);
    }
    public void addRepItem(int goid, int offsetId, float floatVal) {
        RepItem newItem = new RepItem(RepItem.RepFloat);
        newItem.instanceId = goid;
        newItem.offset = offsetId;
        newItem.value = BitConverter.GetBytes(floatVal);
        repItems.Add(newItem);
    }
    public void addRepItem(int goid, int offsetId, int[] intArray) {
        RepItem newItem = new RepItem(RepItem.RepIntArray);
        newItem.instanceId = goid;
        newItem.offset = offsetId;
        //newItem.value = BitConverter.GetBytes(floatVal);
        repItems.Add(newItem);
    }
    byte[] replicateBuffer;
    void replicateBufferToClients() {
        
    }
    void replicateToClients() {
        if (repItems.Count == 0) return;

        byte error;
        ushort opcode = (ushort)NetOpCodes.Replication;
        byte[] intBytes = BitConverter.GetBytes(opcode);
        Array.Copy(intBytes, 0, buffer, 0, intBytes.Length);
        int offset = intBytes.Length;
  
  
        byte[] repItemCountBytes = BitConverter.GetBytes(repItems.Count);
        Array.Copy(repItemCountBytes, 0, buffer, offset, repItemCountBytes.Length);
        offset += repItemCountBytes.Length;

        
        for(int i = 0; i < repItems.Count; ++i) {
            RepItem repItem = repItems[i];
            if (repItem.dataType == RepItem.RepInt) { // value type
                buffer[offset] = repItem.dataType;
                offset++;

                Array.Copy(repItem.value, 0, buffer, offset, 4);
                offset += 4;
            }
            else if (repItem.dataType == RepItem.RepFloat) { // value type
                buffer[offset] = repItem.dataType;
                offset++;

                Array.Copy(repItem.value, 0, buffer, offset, 4);
                offset += 4;
            }
            else if (repItem.dataType == RepItem.RepString) { // ref type
                buffer[offset] = repItem.dataType;
                offset++;
                byte[] stringBytes = Encoding.ASCII.GetBytes(repItem.ptr as string);
                Array.Copy(BitConverter.GetBytes(stringBytes.Length), 0, buffer, offset, 4); // string length
                offset += 4;

                Array.Copy(stringBytes, 0, buffer, offset, stringBytes.Length);
                offset += stringBytes.Length;
            }
            else if (repItem.dataType == RepItem.RepVec2) { // ref type
                buffer[offset] = repItem.dataType;
                offset++;
                Vector2 vec2 = (Vector2)repItem.ptr;
                Array.Copy(BitConverter.GetBytes(vec2.x), 0, buffer, offset, 4);
                offset += 4;
                Array.Copy(BitConverter.GetBytes(vec2.y), 0, buffer, offset, 4);
                offset += 4;
            }
            else if (repItem.dataType == RepItem.RepVec3) { // ref type
                buffer[offset] = repItem.dataType;
                offset++;
                Vector3 vec3 = (Vector3)repItem.ptr;
                Array.Copy(BitConverter.GetBytes(vec3.x), 0, buffer, offset, 4);
                offset += 4;
                Array.Copy(BitConverter.GetBytes(vec3.y), 0, buffer, offset, 4);
                offset += 4;
                Array.Copy(BitConverter.GetBytes(vec3.z), 0, buffer, offset, 4);
                offset += 4;
            }
            else if (repItem.dataType == RepItem.RepIntArray) { // ref type
                buffer[offset] = repItem.dataType;
                offset++;

                int[] intArray = (int[])repItem.ptr;
                Array.Copy(BitConverter.GetBytes(intArray.Length), 0, buffer, offset, 4); // array length
                offset += 4;
                
                for (int j = 0; j < intArray.Length; ++j) {
                    Array.Copy(BitConverter.GetBytes(intArray[j]), 0, buffer, offset, 4);
                    offset += 4;
                }
            }
            else if (repItem.dataType == RepItem.RepFloatArray) { // ref type
                buffer[offset] = repItem.dataType;
                offset++;

                float[] floatArrray = (float[])repItem.ptr;
                Array.Copy(BitConverter.GetBytes(floatArrray.Length), 0, buffer, offset, 4); // array length
                offset += 4;

                for (int j = 0; j < floatArrray.Length; ++j) {
                    Array.Copy(BitConverter.GetBytes(floatArrray[j]), 0, buffer, offset, 4);
                    offset += 4;
                }
            }
            else {
                continue;
            }

            Array.Copy(BitConverter.GetBytes(repItem.instanceId), 0, buffer, offset, 4);
            offset += 4;
            Array.Copy(BitConverter.GetBytes(repItem.offset), 0, buffer, offset, 4);
            offset += 4;
        }
        repItems.Clear();

        for (int i = 0; i < playerControllers.Count; ++i) {
            NetworkTransport.Send(socketId, playerControllers[i], reliableCHN, buffer, offset, out error);
        }
    }
    #endregion
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