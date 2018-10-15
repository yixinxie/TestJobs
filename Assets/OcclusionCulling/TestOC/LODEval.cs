using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODEval : MonoBehaviour {
    public static LODEval self;
    // static
    Vector3[] positions;
    float[] distances;
    byte[] lods;
    private void Awake() {
        self = this;
    }

    public void registerLOD() {

    }
    private void Start() {
        
    }
    // Update is called once per frame
    void Update () {
        Vector3 hotpos = Vector3.zero;
		for(int i = 0; i < positions.Length; ++i) {
            float dist = Vector3.Distance(hotpos, positions[i]);
            if(dist < distances[i]) {

            }
        }
	}
}
