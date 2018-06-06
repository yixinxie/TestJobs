using UnityEngine;

public class ReplicatedProperties : MonoBehaviour {
    public bool alwaysRelevant = true;
    public int owner = -1;
    protected int goId;
    //public byte orderOnGO;
    protected virtual void initNetworking()
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
