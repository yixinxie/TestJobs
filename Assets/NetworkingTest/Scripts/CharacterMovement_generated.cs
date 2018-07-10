
/** auto-generated file. do not modify unless you know what you are doing! */
public partial class CharacterMovement{

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
            onrepServerTime(old_serverTime);
            
            break;

        }
        return true;
    }
    
    /** rpc serializers*/
    
    public void ReceiveUpdate_OnServer(UnityEngine.Vector3 pos,UnityEngine.Vector3 rot,System.Single estTime,UnityEngine.Vector3 frameVelocity,System.Byte interpolationMode){
		ClientTest.self.rpcBegin(goId, 64, SerializedBuffer.RPCMode_Unreliable);
		ClientTest.self.rpcAddParam(pos);
		ClientTest.self.rpcAddParam(rot);
		ClientTest.self.rpcAddParam(estTime);
		ClientTest.self.rpcAddParam(frameVelocity);
		ClientTest.self.rpcAddParam(interpolationMode);
		ClientTest.self.rpcEnd();

    }



    /** rpc reception method(client)*/
    public override bool rpcReceive(ushort rpc_id, byte[] src, ref int offset) {
        if(base.rpcReceive(rpc_id, src, ref offset)) return true;
        switch(rpc_id){
            
            case 64:
            {
                UnityEngine.Vector3 pos = ClientTest.deserializeToVector3(src, ref offset);UnityEngine.Vector3 rot = ClientTest.deserializeToVector3(src, ref offset);System.Single estTime = ClientTest.deserializeToFloat(src, ref offset);UnityEngine.Vector3 frameVelocity = ClientTest.deserializeToVector3(src, ref offset);System.Byte interpolationMode = ClientTest.deserializeToByte(src, ref offset);ReceiveUpdate(pos, rot, estTime, frameVelocity, interpolationMode);
            }
            break;


        }
        return true;
    }
}
