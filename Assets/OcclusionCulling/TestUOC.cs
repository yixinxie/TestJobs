using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUOC : MonoBehaviour {
    public static TestUOC self;
    CullingGroup group0;

    private void Awake() {
        self = this;
        group0 = new CullingGroup();
    }
    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
