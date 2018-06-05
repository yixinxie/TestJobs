using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Generated, Do not modify!*/
public partial class ReplicatedProperties_PlayerController : ReplicatedProperties {
    // Use this for initialization
    
    [Replicated]
    public int testVal;
    //[Replicated(OnRep="testFloatChanged")]
    [Replicated]
    public float testFloat;
    // onrep callbacks
    public Action<int> onRep_owner;
    public Action<int> onRep_testVal;
    public Action<float> onRep_testFloat;

    public Action<string> rpc_ClientChat;

    private void Awake()
    {
        initNetworking();
    }
    //public override void receive(int offset, int newVal) {
    //    if(offset == 0) {
    //        owner = newVal;
    //        int oldVal = newVal;
    //        if (onRep_owner != null) {
    //            onRep_owner(oldVal);
    //        }
    //    }
    //    else if(offset == 1) {
    //        int oldVal = testVal;
    //        testVal = newVal;
    //        if (onRep_testVal != null) {
    //            onRep_testVal(oldVal);
    //        }
    //    }
    //}

    //public override void receive(int offset, float newVal) {
    //    if (offset == 2) {
    //        float oldVal = testVal;
    //        testFloat = newVal;
    //        if (onRep_testFloat != null) {
    //            onRep_testFloat(oldVal);
    //        }
    //    }
    //}



    //public void rep_testVal() {
    //    ServerTest.self.addRepItem(goId, 1, testVal);
    //}
    //public void rep_testFloat() {
    //    ServerTest.self.addRepItem(goId, 2, testFloat);
    //}
    //public void rep_generic()
    //{

    //}

    private void testFloatChanged() {
        throw new NotImplementedException();
    }
    // generate this!
    public void ServerTestRPC(params object[] paramObjs) {
        ServerTest.self.invokeServerRPC("ServerTest_Implementation", paramObjs);
    }

    public void ServerTest_Implementation(params object[] paramObjs) {
    }
}
