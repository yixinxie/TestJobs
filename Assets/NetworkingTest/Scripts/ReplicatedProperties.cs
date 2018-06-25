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
        ServerTest.self.rflcAddInt(goId, 0, owner);
    }

    public virtual bool rpcReceive(ushort rpc_id, byte[] src, ref int offset)
    {
        return false;
    }
}
