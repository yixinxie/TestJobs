
public partial class PlayerState{
    /** call this in Awake() */
    protected override void initNetworking()
    {
        base.initNetworking();
    }

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
		ClientTest.self.rpcParamAddInt(goId, 0, testint);
		ClientTest.self.rpcParamAddFloat(goId, 0, testfloat);

    }


    public void Server_testRPCtwo(System.Int32 testint,System.Single testfloat,System.Single float2){
		ClientTest.self.rpcParamAddInt(goId, 1, testint);
		ClientTest.self.rpcParamAddFloat(goId, 1, testfloat);
		ClientTest.self.rpcParamAddFloat(goId, 1, float2);

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
