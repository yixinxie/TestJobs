using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public partial class CharacterMovement : ReplicatedProperties {
    CharacterController cc;
    protected override void Awake() {
        base.Awake();
        cc = GetComponent<CharacterController>();
    }
    // Use this for initialization
    void Start () {
		
	}
    [RPC]
    public void ServerReceiveUpdate(Vector3 pos, Vector3 rot) {

    }
    public void LateUpdate() {
        //transform.po
        if (role == GameObjectRoles.Autonomous || role == GameObjectRoles.None) {
            //ClientTest.self.r
        }
        else if (role == GameObjectRoles.Authority) {

        }
        else if (role == GameObjectRoles.SimulatedProxy) {

        }
    }
}
