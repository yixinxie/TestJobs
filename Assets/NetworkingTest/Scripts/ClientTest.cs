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
    byte[] buffer;
    byte[] recBuffer;
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
        recBuffer = new byte[1024];
        NetGOs = new Dictionary<int, ReplicatedProperties>();
        int bufferSize = 256;
        buffer = new byte[bufferSize];
        repprops = new Dictionary<int, ReplicatedProperties>();
    }

    private void Start() {
        byte error;
        connectionId = NetworkTransport.Connect(socketId, serverAddr, serverPort, 0, out error);
        Debug.Log("Connected to server. ConnectionId: " + connectionId + " error:" + (int)error);
        //NetworkTransport.Send(hostId, connectionId, reliableCHN, buffer, bufferLength, out error);
    }
    Dictionary<int, ReplicatedProperties> repprops;
    int repprop_inc;
    public int registerReplicatedProperties(ReplicatedProperties repprop)
    {
        int ret = repprop_inc;
        repprops.Add(ret, repprop);
        repprop_inc++;
        return ret;
    }

    public static byte decodeRawData(byte[] src, Dictionary<int, ReplicatedProperties> NetGOByServerInstId) {
        int offset = 0;
        int repItemCount = BitConverter.ToInt32(src, 2);
        for (int j = 0; j < repItemCount; ++j)
        {
            ushort opcode = BitConverter.ToUInt16(src, offset);
            offset += 2;
            if (opcode == (ushort)NetOpCodes.SpawnPrefab)
            {
                int id_count = src[offset];
                offset++;
                int[] serverInstIds = new int[id_count];
                bool found = false;
                for (int i = 0; i < id_count; ++i)
                {
                    serverInstIds[i] = BitConverter.ToInt32(src, offset + i * 4);
                    
                    if(NetGOByServerInstId.ContainsKey(serverInstIds[i]))
                    {
                        found = true;
                        break;

                    }
                }
                offset += 4 * id_count;

                ushort length = BitConverter.ToUInt16(src, offset);
                offset += 2;
                string path = Encoding.ASCII.GetString(src, offset, length);
                offset += length;
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
                int goid = BitConverter.ToInt32(src, offset);
                offset += 4;
                ushort varOffset = BitConverter.ToUInt16(src, offset);
                offset += 2;

                if (dataType == RepItem.RepInt)
                {
                    if (NetGOByServerInstId.ContainsKey(goid))
                    {
                        ReplicatedProperties propComp = NetGOByServerInstId[goid];
                        if (propComp != null)
                        {
                            propComp.receiveGeneric(varOffset, src, ref offset);
                        }
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
                    //else if (dataType == RepItem.RepGameObject) {
                    //    int serverGOInstId = BitConverter.ToInt32(src, offset);
                    //    offset += 4;
                    //    ushort length = BitConverter.ToUInt16(src, offset);
                    //    offset += 2;
                    //    string path = Encoding.ASCII.GetString(src, 8, length);
                    //    offset += length;
                    //    if (NetGOByServerInstId.ContainsKey(serverGOInstId) == false) {
                    //        Debug.Log("spawning " + path);
                    //        UnityEngine.Object o = Resources.Load(path);
                    //        GameObject spawnedGO = GameObject.Instantiate(o) as GameObject;
                    //        NetGOByServerInstId.Add(serverGOInstId, spawnedGO.GetComponent<ReplicatedProperties>());
                    //    }
                    //    else {
                    //        Debug.Log("spawning a prefab from server that already exists: " + path);
                    //    }
                    //}
            }
        }
        return 0;
    }
    public static int deserializeToInt(byte[] src, ref int offset)
    {
        int ret = BitConverter.ToInt32(src, offset);
        offset += 4;
        return ret;
    }

    public static float deserializeToFloat(byte[] src, ref int offset)
    {
        float ret = BitConverter.ToSingle(src, offset);
        offset += 4;
        return ret;
    }

    // client calls an RPC on the server
    public byte callRPCOnServer(ReplicatedProperties netgo, string methodName) {
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

        NetworkTransport.Send(socketId, connectionId, reliableCHN, buffer, offset, out error);
        return error;
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
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
            switch (recData) {
                case NetworkEventType.Nothing:         //1
                    loop = false;
                    break;
                case NetworkEventType.ConnectEvent:    //2
                    Debug.Log(recData.ToString());
                    break;
                case NetworkEventType.DataEvent:       //3
                    decodeRawData(recBuffer, NetGOs);
                    break;
                case NetworkEventType.DisconnectEvent: //4
                    Debug.Log(recData.ToString());
                    break;
            }
        }
    }
}
