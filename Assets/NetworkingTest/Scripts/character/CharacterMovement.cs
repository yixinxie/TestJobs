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
    [RPC(isServer =1)]
    public void ReceiveUpdate(Vector3 pos, Vector3 rot) {
        transform.position = pos;
        transform.eulerAngles = rot;
    }
    public void LateUpdate() {
        //transform.po
        if (role == GameObjectRoles.Autonomous || role == GameObjectRoles.None) {
            ClientTest.self.rpcBegin(goId, 16, SerializedBuffer.RPCMode_ToServer);
            ClientTest.self.rpcAddParam(transform.position);
            ClientTest.self.rpcAddParam(transform.eulerAngles);
            ClientTest.self.rpcEnd();
        }
        else if (role == GameObjectRoles.Authority) {

        }
        else if (role == GameObjectRoles.SimulatedProxy) {

        }
    }
}
