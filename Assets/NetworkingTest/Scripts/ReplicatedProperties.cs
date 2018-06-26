using UnityEngine;
public enum GameObjectRoles : byte {
    None, // disconnected
    SimulatedProxy, // remote client
    Autonomous, // controlling client
    Authority, // server
}
public class ReplicatedProperties : MonoBehaviour {
    
    public int owner = -1;
    protected int goId;
    public bool alwaysRelevant = true;
    public GameObjectRoles role;
    protected virtual void Awake()
    {
        goId = GetInstanceID();
    }
    // called on client
    public virtual bool stateRepReceive(ushort varOffset, byte[] src, ref int offset)
    {
        if(varOffset == 0)
        {
            owner = ClientTest.deserializeToInt(src, ref offset);
            return true;
        }
        return false;
    }

    public void rep_owner()
    {
        ServerTest.self.repInt(goId, 0, owner, RPCMode_ToOwner| RPCMode_ToRemote);
    }
    // called by the server
    public void clientSetRole() {
        ClientTest.self.rpcBegin(goId, 0, ClientTest.RPCMode_ToOwner);
        ClientTest.self.rpcParamAddByte((byte)GameObjectRoles.Autonomous);
        ClientTest.self.rpcEnd();

        ClientTest.self.rpcBegin(goId, 0, ClientTest.RPCMode_ToRemote);
        ClientTest.self.rpcParamAddByte((byte)GameObjectRoles.SimulatedProxy);
        ClientTest.self.rpcEnd();
    }

    public virtual bool rpcReceive(ushort rpc_id, byte[] src, ref int offset)
    {
        bool ret = false;
        switch (rpc_id) {
            case 0:
                role = (GameObjectRoles)ClientTest.deserializeToInt(src, ref offset);
                ret = true;
                break;
        }
        return ret;
    }
}
