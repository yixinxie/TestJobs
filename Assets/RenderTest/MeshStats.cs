using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshStats : MonoBehaviour {
    public MeshFilter mf;
    // Use this for initialization
    private void Awake() {
        mf = GetComponent<MeshFilter>();
    }
    void Start () {
        Debug.Log("mesh bounds: " + mf.mesh.bounds.ToString());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
