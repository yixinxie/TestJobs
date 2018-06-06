using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Generated, Do not modify!*/
public partial class PlayerState : ReplicatedProperties {
    // Use this for initialization
    
    [Replicated]
    public int testVal;
    [Replicated(OnRep="testFloatChanged")]
    public float testFloat;
    // onrep callbacks
    private void Awake()
    {
        initNetworking();
    }
    [OnRep]
    private void testFloatChanged(float oldfloat) {
    }

    [RPC]
    public void testRPC(int testint, float testfloat)
    {

    }

    [RPC]
    public void testRPCtwo(int testint, float testfloat, float float2)
    {

    }
}
