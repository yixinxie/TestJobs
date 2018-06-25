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
	
	// Update is called once per frame
	void Update () {
		
	}
}
