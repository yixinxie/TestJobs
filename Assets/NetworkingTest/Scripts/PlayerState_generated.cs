
public partial class PlayerState{

    /** variable replication methods(server)*/
    
    public void rep_testVal() {
        ServerTest.self.rflcAddInt(goId, 64, testVal);
    }

    public void rep_testFloat() {
        ServerTest.self.rflcAddFloat(goId, 65, testFloat);
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
    
    public void Server_testRPC(System.Int32 testint,System.Single testfloat){
		ClientTest.self.rpcBegin(goId, 0, ClientTest.RPCMode_ToServer);
		ClientTest.self.rpcParamAddInt(testint);
		ClientTest.self.rpcParamAddFloat(testfloat);
		ClientTest.self.rpcEnd();

    }


    public void Server_testRPCtwo(System.Int32 testint,System.Single testfloat,System.Single float2){
		ClientTest.self.rpcBegin(goId, 1, ClientTest.RPCMode_ToServer);
		ClientTest.self.rpcParamAddInt(testint);
		ClientTest.self.rpcParamAddFloat(testfloat);
		ClientTest.self.rpcParamAddFloat(float2);
		ClientTest.self.rpcEnd();

    }



    /** rpc reception method(client)*/
    public override bool rpcReceive(ushort rpc_id, byte[] src, ref int offset) {
        if(base.rpcReceive(rpc_id, src, ref offset)) return true;
        switch(rpc_id){
            
            case 0:
            {
                System.Int32 testint = ClientTest.deserializeToInt(src, ref offset);System.Single testfloat = ClientTest.deserializeToFloat(src, ref offset);testRPC(testint, testfloat);
            }
            break;


            case 1:
            {
                System.Int32 testint = ClientTest.deserializeToInt(src, ref offset);System.Single testfloat = ClientTest.deserializeToFloat(src, ref offset);System.Single float2 = ClientTest.deserializeToFloat(src, ref offset);testRPCtwo(testint, testfloat, float2);
            }
            break;


        }
        return true;
    }
}
