
/** auto-generated file. do not modify unless you know what you are doing! */
public partial class FPSCharacter{

    /** variable replication methods(server)*/
    
    
    /** replicate all states upon gameobject replication*/
    public override void replicateAllStates(byte repMode, int conn_id = -1) {
        base.replicateAllStates(repMode, conn_id);
        
    }

    /** variable reception method(client)*/
    /*
    public override bool stateRepReceive(ushort varOffset, byte[] src, ref int offset) {
        if(base.stateRepReceive(varOffset, src, ref offset)) return true;
        switch(varOffset){
            %rep_switch_body%
        }
        return true;
    }
    */
    
    /** rpc serializers*/
    

    /** rpc reception method(client)*/
    /*
    public override bool rpcReceive(ushort rpc_id, byte[] src, ref int offset) {
        if(base.rpcReceive(rpc_id, src, ref offset)) return true;
        switch(rpc_id){
            %rpc_switch_body%
        }
        return true;
    }
    */
}
