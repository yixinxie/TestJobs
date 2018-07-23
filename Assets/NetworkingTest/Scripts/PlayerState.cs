using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerState : ReplicatedProperties {
    // Use this for initialization
    public string characterPrefabPath;
    [Replicated]
    public float serverTime;
    public bool isHost;
    // onrep callbacks
    /** call this in Awake() */
    protected override void Awake() {
        base.Awake();
    }
    [OnRep(forVar = "serverTime")]
    private void onrep_ServerTime(float oldfloat) {

    }

    private void Start() {
        if(ServerTest.self != null) {
            if (isHost) {
                ServerTest.self.createServerGameObject(characterPrefabPath);
            }
            else {
                
                GameObject psGO = ServerTest.self.spawnReplicatedGameObject(owner, characterPrefabPath);
                serverTime = CharacterMovement.getTime();
                rep_serverTime();
            }
        }
    }
}
