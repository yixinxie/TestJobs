using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODEval : MonoBehaviour {
    // static
    Vector3[] positions;
    float[] distances;
    
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
