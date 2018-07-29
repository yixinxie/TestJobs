using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// non player character
public partial class NPCharacter : ReplicatedProperties {
    CharacterMovement charMovement;
    public Transform MeshGO;
    //public WeaponController currentWeapon;
    public float gravity = 9.8f;
    protected override void Awake() {
        base.Awake();
        charMovement = GetComponent<CharacterMovement>();
        this.enabled = false;
        charMovement.enabled = false;
    }
    private void Start() {
        
        PendingNetworkObjects.self.registerNPC(this);
    }
    // Use this for initialization
    public override void initialReplicationComplete() {
        base.initialReplicationComplete();
    }
    
    // Update is called once per frame
    void Update () {
        float deltaTime = Time.deltaTime;
        charMovement.update(deltaTime);

    }
    
}
