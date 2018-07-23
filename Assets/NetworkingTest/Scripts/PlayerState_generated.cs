
/** auto-generated file. do not modify unless you know what you are doing! */
public partial class PlayerState{

    /** variable replication methods(server)*/
    
    public void rep_serverTime() {
        ServerTest.self.repVar(goId, 64, serverTime, SerializedBuffer.RPCMode_ToTarget | SerializedBuffer.RPCMode_ExceptTarget);
    }

    
    /** replicate all states upon gameobject replication*/
    public override void replicateAllStates(byte repMode, int conn_id = -1) {
        base.replicateAllStates(repMode, conn_id);
        rep_serverTime();
    }

    /** variable reception method(client)*/
    
    public override bool stateRepReceive(ushort varOffset, byte[] src, ref int offset) {
        if(base.stateRepReceive(varOffset, src, ref offset)) return true;
        switch(varOffset){
            
            case 64:
            float old_serverTime = serverTime;
            serverTime = ClientTest.deserializeToFloat(src, ref offset);
            onrep_ServerTime(old_serverTime);
            
            break;

        }
        return true;
    }
    
    
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
