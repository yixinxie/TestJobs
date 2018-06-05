using UnityEngine;

public class ReplicatedProperties : MonoBehaviour {
    public bool alwaysRelevant = true;
    public int owner = -1;
    protected int goId;
    //public byte orderOnGO;
    protected virtual void initNetworking()
    {
        goId = GetInstanceID();
        if (ClientTest.self != null)
        {
            ClientTest.self.registerReplicatedProperties(this);
        }
        if(ServerTest.self != null)
        {

        }
        
    }
    // called on client
    public virtual void receive(int offset, int newVal) {

    }
    public virtual void receiveGeneric(ushort varOffset, byte[] src, ref int offset)
    {

    }

    public void rep_owner()
    {
        ServerTest.self.rflcAddInt(goId, 0, owner);
    }
    
}
