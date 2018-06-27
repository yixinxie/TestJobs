
public partial class PlayerState{

    /** variable replication methods(server)*/
    
    public void rep_testVal() {
        ServerTest.self.repVar(goId, 64, testVal, SerializedBuffer.RPCMode_ToOwner | SerializedBuffer.RPCMode_ToRemote);
    }

    public void rep_testFloat() {
        ServerTest.self.repVar(goId, 65, testFloat, SerializedBuffer.RPCMode_ToOwner | SerializedBuffer.RPCMode_ToRemote);
    }


    /** variable reception method(client)*/
    public override bool stateRepReceive(ushort varOffset, byte[] src, ref int offset) {
        if(base.stateRepReceive(varOffset, src, ref offset)) return true;
        switch(varOffset){
            
            case 64:
            //int old_testVal = testVal;
            testVal = ClientTest.deserializeToInt(src, ref offset);
            //(old_testVal);
            
            break;

            case 65:
            //float old_testFloat = testFloat;
            testFloat = ClientTest.deserializeToFloat(src, ref offset);
            //(old_testFloat);
            
            break;

        }
        return true;
    }
    
    /** rpc serializers*/
    
    public void testRPC_OnServer(System.Int32 testint,System.Single testfloat){
		ClientTest.self.rpcBegin(goId, 64, SerializedBuffer.RPCMode_ToServer);
		ClientTest.self.rpcAddParam(testint);
		ClientTest.self.rpcAddParam(testfloat);
		ClientTest.self.rpcEnd();

    }


    public void testRPCtwo_OnClient(System.Int32 testint,System.Single testfloat,System.Single float2){
		ServerTest.self.rpcBegin(goId, 65, SerializedBuffer.RPCMode_ToOwner | SerializedBuffer.RPCMode_ToRemote);
		ServerTest.self.rpcAddParam(testint);
		ServerTest.self.rpcAddParam(testfloat);
		ServerTest.self.rpcAddParam(float2);
		ServerTest.self.rpcEnd();

    }



    /** rpc reception method(client)*/
    public override bool rpcReceive(ushort rpc_id, byte[] src, ref int offset) {
        if(base.rpcReceive(rpc_id, src, ref offset)) return true;
        switch(rpc_id){
            
            case 64:
            {
                System.Int32 testint = ClientTest.deserializeToInt(src, ref offset);System.Single testfloat = ClientTest.deserializeToFloat(src, ref offset);testRPC(testint, testfloat);
            }
            break;


            case 65:
            {
                System.Int32 testint = ClientTest.deserializeToInt(src, ref offset);System.Single testfloat = ClientTest.deserializeToFloat(src, ref offset);System.Single float2 = ClientTest.deserializeToFloat(src, ref offset);testRPCtwo(testint, testfloat, float2);
            }
            break;


        }
        return true;
    }
}
