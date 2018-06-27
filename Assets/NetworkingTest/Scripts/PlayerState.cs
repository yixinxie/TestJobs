using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Generated, Do not modify!*/
public partial class PlayerState : ReplicatedProperties {
    // Use this for initialization
    public string characterPrefabPath;
    [Replicated]
    public int testVal;
    [Replicated]
    public float testFloat;
    // onrep callbacks
    /** call this in Awake() */
    protected override void Awake() {
        base.Awake();
    }
    [OnRep(forVar = "testFloat2")]
    private void testFloatChanged(float oldfloat) {
    }

    [RPC]
    public void testRPC(int testint, float testfloat)
    {

    }

    [RPC(isServer = 0)]
    public void testRPCtwo(int testint, float testfloat, float float2)
    {

    }
    private void Start() {
        if(ServerTest.self != null) {
            ServerTest.self.spawnReplicatedGameObject(owner, characterPrefabPath);
        }
    }
}
