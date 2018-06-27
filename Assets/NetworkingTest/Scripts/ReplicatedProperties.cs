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
        ServerTest.self.repVar(goId, 0, owner, SerializedBuffer.RPCMode_ToOwner| SerializedBuffer.RPCMode_ToRemote);
    }
    // called by the server
    public void clientSetRole() {
        ServerTest.self.rpcBegin(goId, 0, SerializedBuffer.RPCMode_ToOwner);
        ServerTest.self.rpcAddParam((byte)GameObjectRoles.Autonomous);
        ServerTest.self.rpcEnd();

        ServerTest.self.rpcBegin(goId, 0, SerializedBuffer.RPCMode_ToRemote);
        ServerTest.self.rpcAddParam((byte)GameObjectRoles.SimulatedProxy);
        ServerTest.self.rpcEnd();
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
