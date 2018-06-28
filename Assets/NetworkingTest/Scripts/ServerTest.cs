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
public class OwnedGameObject {
    public List<GameObject> ownedObjects;
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
    List<List<GameObject>> gameObjectsByOwner; // player controller(connection) id, owned NetGOs.
    Dictionary<int, ReplicatedProperties> synchronizedComponents; // instance id, game object
    List<int> newConnectionList;
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
        sendBuffers = new List<SerializedBuffer>(defaultMaxPlayerCount);
        synchronizedComponents = new Dictionary<int, ReplicatedProperties>();
        gameObjectsByOwner = new List<List<GameObject>>();
        newConnectionList = new List<int>();
    }

    private void Start() {
    }

    public GameObject spawnReplicatedGameObject(int connId, string path) {
        int[] goid;
        GameObject pcgo = spawnPrefabOnServer(connId, path, out goid);

        serializeGameObjectReplication(goid, path);

        ReplicatedProperties[] repComponents = pcgo.GetComponents<ReplicatedProperties>();
        for (int i = 0; i < repComponents.Length; ++i) {
            repComponents[i].replicateAllStates(SerializedBuffer.RPCMode_ToOwner | SerializedBuffer.RPCMode_ToRemote);
            repComponents[i].clientSetRole();
        }

        for (int i = 0; i < playerStates.Count; ++i) {
            if(playerStates[i] == connId) {
                gameObjectsByOwner[i].Add(pcgo);
            }
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
            comp_ids[i] = repComponents[i].GetInstanceID();
            synchronizedComponents.Add(comp_ids[i], repComponents[i]);
            repComponents[i].owner = connId;
        }
        
        return newNetGO;
    }

    void serializeGameObjectReplication(int[] component_ids, string path)
    {
        for (int i = 0; i < playerStates.Count; ++i)
        {
            //spawnNetGameObjectOnSingleRemote2(goid, path, playerStates[i]);

            // 2    1   4x?   2     ?
            sendBuffers[i].serializeUShort((ushort)NetOpCodes.SpawnPrefab);
            sendBuffers[i].serializeByte((byte)component_ids.Length);
            // instance id count
            for (int j = 0; j < component_ids.Length; ++j) {
                sendBuffers[i].serializeInt(component_ids[j]);
            }
            sendBuffers[i].serializeString(path);
            sendBuffers[i].incrementCommandCount();
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
        
        int bufferSize = 1024;
        int dataSize;
        byte error;
        newConnectionList.Clear();
        for (int i = 0; i < 10 && loop; ++i) {
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out recConnectionId, out channelId, recvBuffer, bufferSize, out dataSize, out error);
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
                                sendBuffers.RemoveAt(j);
                                List<GameObject> toRemove = gameObjectsByOwner[j];
                                for(int k = 0; k < toRemove.Count; ++k) {
                                    GameObject.Destroy(toRemove[k]);
                                }
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
            gameObjectsByOwner.Add(new List<GameObject>());
            spawnReplicatedGameObject(newConnectionList[i], PlayerStatePrefabPath);
        }
        replicateToClients();
    }
    public void repVar(int component_id, ushort var_id, int val, byte rep_mode) {
        for (int i = 0; i < playerStates.Count; ++i) {
            sendBuffers[i].repInt(component_id, var_id, val);
        }
    }
    public void repVar(int component_id, ushort var_id, float val, byte rep_mode) {
        for (int i = 0; i < playerStates.Count; ++i) {
            sendBuffers[i].repFloat(component_id, var_id, val);
        }
    }
    byte rpcSessionMode;
    int rpcSessionOwner;
    public void rpcBegin(int component_id, ushort rpc_id, byte mode, int owner_id) {
        rpcSessionMode = mode;
        rpcSessionOwner = owner_id;
        if ((rpcSessionMode & SerializedBuffer.RPCMode_ToOwner ) > 0) {
            for(int i = 0; i < playerStates.Count; ++i) {
                if(playerStates[i] == rpcSessionOwner) {
                    sendBuffers[i].rpcBegin(component_id, rpc_id, rpcSessionMode);
                    break;
                }
            }
        }
        else if ((rpcSessionMode & SerializedBuffer.RPCMode_ToRemote) > 0) {
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerStates[i] != rpcSessionOwner) {
                    sendBuffers[i].rpcBegin(component_id, rpc_id, rpcSessionMode);
                    break;
                }
            }
        }
    }
    public void rpcEnd() {
        if ((rpcSessionMode & SerializedBuffer.RPCMode_ToOwner) > 0) {
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerStates[i] == rpcSessionOwner) {
                    sendBuffers[i].rpcEnd();
                    break;
                }
            }
        }
        else if ((rpcSessionMode & SerializedBuffer.RPCMode_ToRemote) > 0) {
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerStates[i] != rpcSessionOwner) {
                    sendBuffers[i].rpcEnd();
                    break;
                }
            }
        }
    }
    public void rpcAddParam(byte val) {
        
        //if ((rpcSessionMode & SerializedBuffer.RPCMode_ToOwner) > 0
        //    && (rpcSessionMode & SerializedBuffer.RPCMode_ToRemote) > 0) {
        //    toAllBuffer.rpcAddParam(val);
        //}
        //else 
        if ((rpcSessionMode & SerializedBuffer.RPCMode_ToOwner) > 0) {
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerStates[i] == rpcSessionOwner) {
                    sendBuffers[i].rpcAddParam(val);
                    break;
                }
            }
        }
        else if ((rpcSessionMode & SerializedBuffer.RPCMode_ToRemote) > 0) {
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerStates[i] != rpcSessionOwner) {
                    sendBuffers[i].rpcAddParam(val);
                    break;
                }
            }
        }
    }
    public void rpcAddParam(int val) {
        if ((rpcSessionMode & SerializedBuffer.RPCMode_ToOwner) > 0) {
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerStates[i] == rpcSessionOwner) {
                    sendBuffers[i].rpcAddParam(val);
                    break;
                }
            }
        }
        else if ((rpcSessionMode & SerializedBuffer.RPCMode_ToRemote) > 0) {
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerStates[i] != rpcSessionOwner) {
                    sendBuffers[i].rpcAddParam(val);
                    break;
                }
            }
        }
    }
    public void rpcAddParam(float val) {
        if ((rpcSessionMode & SerializedBuffer.RPCMode_ToOwner) > 0) {
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerStates[i] == rpcSessionOwner) {
                    sendBuffers[i].rpcAddParam(val);
                    break;
                }
            }
        }
        else if ((rpcSessionMode & SerializedBuffer.RPCMode_ToRemote) > 0) {
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerStates[i] != rpcSessionOwner) {
                    sendBuffers[i].rpcAddParam(val);
                    break;
                }
            }
        }
    }
    public void rpcAddParam(Vector3 val) {
        if ((rpcSessionMode & SerializedBuffer.RPCMode_ToOwner) > 0) {
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerStates[i] == rpcSessionOwner) {
                    sendBuffers[i].rpcAddParam(val);
                    break;
                }
            }
        }
        else if ((rpcSessionMode & SerializedBuffer.RPCMode_ToRemote) > 0) {
            for (int i = 0; i < playerStates.Count; ++i) {
                if (playerStates[i] != rpcSessionOwner) {
                    sendBuffers[i].rpcAddParam(val);
                    break;
                }
            }
        }
    }

    void replicateToClients()
    {
        byte error = 0;
        // to each
        for (int i = 0; i < playerStates.Count; ++i)
        {
            if (sendBuffers[i].getCommandCount() > 0) {
                sendBuffers[i].seal();
                NetworkTransport.Send(socketId, playerStates[i], reliableCHN, sendBuffers[i].getBuffer(), sendBuffers[i].getOffset(), out error);
                sendBuffers[i].reset();
            }
        }
    }
}