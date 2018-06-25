using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
public class ClientTest : MonoBehaviour
{
    public static ClientTest self;
    public string serverAddr = "127.0.0.1";
    // Use this for initialization
    int serverPort = 427;
    public int clientPort = 221;
    int socketId;
    int connectionId;
    int reliableCHN;
    int unreliableCHN;
    int serverConId;
    byte[] sendBuffer;
    byte[] recvBuffer;
    public bool runTest = false;
    Dictionary<int, ReplicatedProperties> NetGOs;

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
        socketId = NetworkTransport.AddHost(topology, clientPort);
        Debug.Log("Socket Open. SocketId is: " + socketId);
        recvBuffer = new byte[1024];
        NetGOs = new Dictionary<int, ReplicatedProperties>();
        int bufferSize = 1024;
        sendBuffer = new byte[bufferSize];

        rpcSerializationOffset = 2;
    }

    private void Start() {
        byte error;
        connectionId = NetworkTransport.Connect(socketId, serverAddr, serverPort, 0, out error);
        Debug.Log("Connected to server. ConnectionId: " + connectionId + " error:" + (int)error);
        
    }
    void sendRPCs() {
        byte error = 0;
        if (rpcCount == 0) return;
        int zero = 0;
        serializeUShort(sendBuffer, (ushort)rpcCount, ref zero);
        NetworkTransport.Send(serverConId, connectionId, reliableCHN, sendBuffer, rpcSerializationOffset, out error);
        rpcSerializationOffset = 2;
        rpcCount = 0;
    }
    public static byte decodeRawData(byte[] src, Dictionary<int, ReplicatedProperties> NetGOByServerInstId) {
        int offset = 0;
        int repItemCount = deserializeToInt(src, ref offset);
        for (int j = 0; j < repItemCount; ++j)
        {
            ushort opcode = deserializeToUShort(src, ref offset);
            if (opcode == (ushort)NetOpCodes.SpawnPrefab)
            {
                int id_count = src[offset];
                offset++;
                int[] serverInstIds = new int[id_count];
                bool found = false;
                int tmpOffset = offset;
                for (int i = 0; i < id_count; ++i)
                {
                    serverInstIds[i] = deserializeToInt(src, ref tmpOffset);
                    if(NetGOByServerInstId.ContainsKey(serverInstIds[i]))
                    {
                        found = true;
                        break;
                    }
                }
                offset += 4 * id_count;

                string path = deserializeToString(src, ref offset);
                if (found == false)
                {
                    Debug.Log("spawning " + path);
                    UnityEngine.Object o = Resources.Load(path);
                    GameObject spawnedGO = GameObject.Instantiate(o) as GameObject;
                    ReplicatedProperties[] rep_components = spawnedGO.GetComponents<ReplicatedProperties>();

                    for (int i = 0; i < id_count; ++i)
                    {
                        NetGOByServerInstId.Add(serverInstIds[i], rep_components[i]);
                    }
                }
                else
                {
                    Debug.Log("spawning a prefab from server that already exists: " + path);
                }
            }
            else if (opcode == (ushort)NetOpCodes.RPCFunc)
            {
                //int serverGOInstId = BitConverter.ToInt32(src, 2);
                //if (NetGOByServerInstId.ContainsKey(serverGOInstId)) {
                //    ushort length = BitConverter.ToUInt16(src, 6);
                //    string methodName = Encoding.ASCII.GetString(src, 8, length);
                //    if (string.IsNullOrEmpty(methodName) == false)
                //        NetGOByServerInstId[serverGOInstId].SendMessage(methodName);
                //}
            }
            else if (opcode == (ushort)NetOpCodes.Replication)
            {
                
                byte dataType = src[offset];
                offset++;
                ushort totalLength= deserializeToUShort(src, ref offset);
                int bkLength = offset;
                int component_id = deserializeToInt(src, ref offset);
                ushort varOffset = deserializeToUShort(src, ref offset);

                if (dataType == RepItem.RepInt)
                {
                    if (NetGOByServerInstId.ContainsKey(component_id))
                    {
                        ReplicatedProperties propComp = NetGOByServerInstId[component_id];
                        if (propComp != null)
                        {
                            propComp.stateRepReceive(varOffset, src, ref offset);
                        }
                        else {
                            Debug.Log("the server is trying to replicate a property to a component that no longer exists.");
                            offset = bkLength + totalLength;
                        }
                    }
                    else {
                        Debug.Log("the server is trying to replicate a property to a component that no longer exists.");
                        offset = bkLength + totalLength;
                    }
                    
                }
                else if (dataType == RepItem.RepFloat)
                {
                }
                else if (dataType == RepItem.RepIntArray)
                {
                    //int arrayCount = BitConverter.ToInt32(src, offset);
                    //offset += 4;
                    //int[] newIntArray = new int[arrayCount];
                    //for (int j = 0; j < arrayCount; ++j)
                    //{
                    //    newIntArray[j] = BitConverter.ToInt32(src, offset);
                    //    offset += 4;
                    //}

                    //int goid = BitConverter.ToInt32(src, offset);
                    //offset += 4;
                    //int varOffset = BitConverter.ToInt32(src, offset);
                    //offset += 4;

                    //if (NetGOByServerInstId.ContainsKey(goid))
                    //{
                    //    ReplicatedProperties propComp = NetGOByServerInstId[goid];
                    //    if (propComp != null)
                    //    {
                    //        propComp.receive(varOffset, newIntArray);
                    //    }
                    //}
                }
            }
        }
        return 0;
    }
    public static void serializeByte(byte[] src, byte byteVal, ref int offset) {
        src[offset] = byteVal;
        offset++;
    }
    public static void serializeInt(byte[] src, int intVal, ref int offset) {
        byte[] intRaw = BitConverter.GetBytes(intVal);
        Array.Copy(intRaw, 0, src, offset, 4);
        offset += 4;
    }
    public static void serializeUShort(byte[] src, ushort ushortVal, ref int offset) {
        byte[] ushortRaw = BitConverter.GetBytes(ushortVal);
        Array.Copy(ushortRaw, 0, src, offset, 2);
        offset += 2;
    }
    public static void serializeFloat(byte[] src, float floatVal, ref int offset) {
        byte[] floatRaw = BitConverter.GetBytes(floatVal);
        Array.Copy(floatRaw, 0, src, offset, 4);
        offset += 4;
    }
    public static void serializeString(byte[] src, string strVal, ref int offset) {
        // path string length
        byte[] lengthBytes = BitConverter.GetBytes((ushort)strVal.Length);
        Array.Copy(lengthBytes, 0, src, offset, 2);
        offset += 2;

        // path string
        byte[] stringBytes = Encoding.ASCII.GetBytes(strVal);
        Array.Copy(stringBytes, 0, src, offset, stringBytes.Length);
        offset += stringBytes.Length;
    }

    public static int deserializeToInt(byte[] src, ref int offset)
    {
        int ret = BitConverter.ToInt32(src, offset);
        offset += 4;
        return ret;
    }
    public static ushort deserializeToUShort(byte[] src, ref int offset) {
        ushort ret = BitConverter.ToUInt16(src, offset);
        offset += 2;
        return ret;
    }
    public static string deserializeToString(byte[] src, ref int offset) {
        ushort length = BitConverter.ToUInt16(src, offset);
        offset += 2;
        string ret = Encoding.ASCII.GetString(src, offset, length);
        offset += length;
        return ret;
    }

    public static float deserializeToFloat(byte[] src, ref int offset)
    {
        float ret = BitConverter.ToSingle(src, offset);
        offset += 4;
        return ret;
    }
    public const byte RPCMode_None = 0;
    public const byte RPCMode_ToServer = 1; // 0001
    public const byte RPCMode_ToClient = 2; // 0010
    public const byte RPCMode_ToOwner = 4; //  0100
    public const byte RPCMode_ToRemote = 8; //  1000
    /** rpc states */
    byte rpcMode;
    int component_id;
    ushort rpcCount;
    int rpcSerializationOffset;
    int rpcSerializationLength;
    public void rpcBegin(int component_id, ushort rpc_id, byte _rpcMode) {
        rpcMode = _rpcMode;
        
        serializeUShort(sendBuffer, (ushort)NetOpCodes.RPCFunc, ref rpcSerializationOffset);
        serializeInt(sendBuffer, component_id, ref rpcSerializationOffset);
        rpcSerializationLength = rpcSerializationOffset;
        serializeUShort(sendBuffer, rpc_id, ref rpcSerializationOffset);
    }
    public void rpcEnd() {
        rpcMode = RPCMode_None;
        serializeUShort(sendBuffer, (ushort)rpcSerializationOffset, ref rpcSerializationLength);
        rpcCount++;
    }
    public void rpcParamAddUShort(ushort ushortVal)
    {
        serializeUShort(sendBuffer, ushortVal, ref rpcSerializationOffset);
    }

    public void rpcParamAddInt(int intVal) {
        serializeInt(sendBuffer, intVal, ref rpcSerializationOffset);
    }

    public void rpcParamAddFloat(float floatVal)
    {
        serializeFloat(sendBuffer, floatVal, ref rpcSerializationOffset);
    }
    private void OnDestroy() {
        byte error;
        NetworkTransport.Disconnect(socketId, connectionId, out error);
    }
    // Update is called once per frame
    void Update () {
        if (runTest) {
            runTest = false;
        }
        int recHostId;
        int connectionId;
        int channelId;
        
        int bufferSize = 1024;
        int dataSize;
        byte error;

        bool loop = true;
        for (int i = 0; i < 10 && loop; ++i) {
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recvBuffer, bufferSize, out dataSize, out error);
            switch (recData) {
                case NetworkEventType.Nothing:         //1
                    loop = false;
                    break;
                case NetworkEventType.ConnectEvent:    //2
                    serverConId = recHostId;
                    Debug.Log(recData.ToString());
                    break;
                case NetworkEventType.DataEvent:       //3
                    decodeRawData(recvBuffer, NetGOs);
                    break;
                case NetworkEventType.DisconnectEvent: //4
                    Debug.Log(recData.ToString());
                    break;
            }
        }
        sendRPCs();
    }
}
