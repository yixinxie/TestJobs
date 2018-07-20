using UnityEngine;
public enum GameObjectRoles : byte {
    None, // disconnected
    SimulatedProxy, // remote client
    Autonomous, // controlling client
    Authority, // server
    Host, // host
    Undefined = 255,
}
/*
 * server-client architecture
 * authority
 * autonomous,  simulated proxy
 * 
 * 
 * host-guest architecture
 * host object
 * on host: authority
 * on guest: simulated proxy
 * 
 * guest object
 * on host: authority
 * on guest: autonomous, simulated proxy
 */
public class ReplicatedProperties : MonoBehaviour {

    public int owner = -65535;
    protected int goId; // id on server.
    public bool alwaysRelevant = true; // not used!
    public GameObjectRoles role = GameObjectRoles.Undefined;
    public int server_id { get { return goId; } }
    protected virtual void Awake()
    {
        goId = GetInstanceID(); // should only run on server.
    }
    // called on client
    public virtual bool stateRepReceive(ushort varOffset, byte[] src, ref int offset)
    {
        if(varOffset == 0)
        {
            owner = ClientTest.deserializeToInt(src, ref offset);
            return true;
        }
        else if (varOffset == 1) {
            goId = ClientTest.deserializeToInt(src, ref offset);
            return true;
        }
        return false;
    }
    // replicate the essential variables.
    public virtual void replicateAllStates(byte repMode, int conn_id = -1)
    {
        ServerTest.self.repVar(goId, 0, owner, repMode, conn_id);
        ServerTest.self.repVar(goId, 1, goId, repMode, conn_id);
    }
    // called by the server, use rpc to set up the corresponding roles on the owner client and remote clients.
    public void clientSetRole(int conn_id = -1) {
        if (conn_id == -1) {
            role = GameObjectRoles.Authority;
            ServerTest.self.rpcBegin(goId, 0, SerializedBuffer.RPCMode_ToTarget, owner);
            ServerTest.self.rpcAddParam((byte)GameObjectRoles.Autonomous);
            ServerTest.self.rpcEnd();

            ServerTest.self.rpcBegin(goId, 0, SerializedBuffer.RPCMode_ExceptTarget, owner);
            ServerTest.self.rpcAddParam((byte)GameObjectRoles.SimulatedProxy);
            ServerTest.self.rpcEnd();

            // send an event to indicate the initial replication batch is completed.
            ServerTest.self.rpcBegin(goId, 1, SerializedBuffer.RPCMode_ToTarget | SerializedBuffer.RPCMode_ExceptTarget, owner);
            ServerTest.self.rpcEnd();
        }
        else {
            // call rpc on a single client.
            ServerTest.self.rpcBegin(goId, 0, SerializedBuffer.RPCMode_ToTarget, conn_id);
            ServerTest.self.rpcAddParam((byte)GameObjectRoles.SimulatedProxy);
            ServerTest.self.rpcEnd();

            // send an event to indicate the initial replication batch is completed.
            ServerTest.self.rpcBegin(goId, 1, SerializedBuffer.RPCMode_ToTarget, conn_id);
            ServerTest.self.rpcEnd();
        }
    }

    protected virtual void initialReplicationComplete() {

    }

    public virtual bool rpcReceive(ushort rpc_id, byte[] src, ref int offset)
    {
        bool ret = false;
        switch (rpc_id) {
            case 0:
                role = (GameObjectRoles)ClientTest.deserializeToByte(src, ref offset);
                Debug.Log(GetType().ToString() + " clientSetRole:" + role.ToString());
                ret = true;
                break;
            case 1:
                initialReplicationComplete();
                // now that the function is called on the client, call this function on the server.
                ClientTest.self.rpcBegin(goId, 2);
                ClientTest.self.rpcEnd();

                ret = true;
                break;
            case 2: // ack 1
                // call this function on the server.
                initialReplicationComplete();
                ret = true;
                break;

        }
        return ret;
    }
}
