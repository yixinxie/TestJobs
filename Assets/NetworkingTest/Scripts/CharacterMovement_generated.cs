
public partial class CharacterMovement{

    /** variable replication methods(server)*/
    

    /** variable reception method(client)*/
    public override bool stateRepReceive(ushort varOffset, byte[] src, ref int offset) {
        if(base.stateRepReceive(varOffset, src, ref offset)) return true;
        switch(varOffset){
            
        }
        return true;
    }
    
    /** rpc serializers*/
    
    public void ReceiveUpdate_OnServer(UnityEngine.Vector3 pos,UnityEngine.Vector3 rot){
		ClientTest.self.rpcBegin(goId, 64);
		ClientTest.self.rpcAddParam(pos);
		ClientTest.self.rpcAddParam(rot);
		ClientTest.self.rpcEnd();

    }



    /** rpc reception method(client)*/
    public override bool rpcReceive(ushort rpc_id, byte[] src, ref int offset) {
        if(base.rpcReceive(rpc_id, src, ref offset)) return true;
        switch(rpc_id){
            
            case 64:
            {
                UnityEngine.Vector3 pos = ClientTest.deserializeToVector3(src, ref offset);UnityEngine.Vector3 rot = ClientTest.deserializeToVector3(src, ref offset);ReceiveUpdate(pos, rot);
            }
            break;


        }
        return true;
    }
}
