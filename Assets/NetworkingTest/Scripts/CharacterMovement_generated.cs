
/** auto-generated file. do not modify unless you know what you are doing! */
public partial class CharacterMovement{

    /** variable replication methods(server)*/
    
    public void rep_serverTime() {
        ServerTest.self.repVar(goId, 64, serverTime, SerializedBuffer.RPCMode_ToTarget | SerializedBuffer.RPCMode_ExceptTarget);
    }

    public void rep_isHost() {
        ServerTest.self.repVar(goId, 65, isHost, SerializedBuffer.RPCMode_ToTarget | SerializedBuffer.RPCMode_ExceptTarget);
    }

    
    /** replicate all states upon gameobject replication*/
    public override void replicateAllStates(byte repMode, int conn_id = -1) {
        base.replicateAllStates(repMode, conn_id);
        rep_serverTime();rep_isHost();
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

            case 65:
            //byte old_isHost = isHost;
            isHost = ClientTest.deserializeToByte(src, ref offset);
            //(old_isHost);
            
            break;

        }
        return true;
    }
    
    
    /** rpc serializers*/
    
    public void ReceiveUpdate_OnServer(UnityEngine.Vector3 pos,UnityEngine.Vector3 rot,System.Single estTime,UnityEngine.Vector3 _frameVelocity,System.Byte interpolationMode){
		ClientTest.self.rpcBegin(goId, 64, SerializedBuffer.RPCMode_Unreliable);
		ClientTest.self.rpcAddParam(pos);
		ClientTest.self.rpcAddParam(rot);
		ClientTest.self.rpcAddParam(estTime);
		ClientTest.self.rpcAddParam(_frameVelocity);
		ClientTest.self.rpcAddParam(interpolationMode);
		ClientTest.self.rpcEnd();

    }


    public void PingServer_OnServer(System.Int32 id){
		ClientTest.self.rpcBegin(goId, 65, SerializedBuffer.RPCMode_Unreliable);
		ClientTest.self.rpcAddParam(id);
		ClientTest.self.rpcEnd();

    }


    public void PingClient_OnClient(System.Int32 id){
		ServerTest.self.rpcBegin(goId, 66, SerializedBuffer.RPCMode_ToTarget | SerializedBuffer.RPCMode_ExceptTarget | SerializedBuffer.RPCMode_Unreliable, owner);
		ServerTest.self.rpcAddParam(id);
		ServerTest.self.rpcEnd();

    }



    /** rpc reception method(client)*/
    
    public override bool rpcReceive(ushort rpc_id, byte[] src, ref int offset) {
        if(base.rpcReceive(rpc_id, src, ref offset)) return true;
        switch(rpc_id){
            
            case 64:
            {
                UnityEngine.Vector3 pos = ClientTest.deserializeToVector3(src, ref offset);UnityEngine.Vector3 rot = ClientTest.deserializeToVector3(src, ref offset);System.Single estTime = ClientTest.deserializeToFloat(src, ref offset);UnityEngine.Vector3 _frameVelocity = ClientTest.deserializeToVector3(src, ref offset);System.Byte interpolationMode = ClientTest.deserializeToByte(src, ref offset);ReceiveUpdate(pos, rot, estTime, _frameVelocity, interpolationMode);
            }
            break;


            case 65:
            {
                System.Int32 id = ClientTest.deserializeToInt(src, ref offset);PingServer(id);
            }
            break;


            case 66:
            {
                System.Int32 id = ClientTest.deserializeToInt(src, ref offset);PingClient(id);
            }
            break;


        }
        return true;
    }
    
}
