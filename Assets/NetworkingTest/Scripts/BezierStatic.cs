using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Simulation_OOP;
public class BezierStatic : MonoBehaviour {
    public Transform[] targets;
    Vector3[] positions;
    Vector3[] four;
    public static Vector3 evalBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        //Vector3 ret;
        //ret.x = Mathf.Pow(1 - t, 3f) * p0.x + 3f * Mathf.Pow(1 - t, 2f) * t * p1.x + 3f * (1 - t) * Mathf.Pow(t, 2f) * p2.x + 3 * Mathf.Pow(t, 3f) * p3.x;
        //ret.y = Mathf.Pow(1 - t, 3f) * p0.y + 3f * Mathf.Pow(1 - t, 2f) * t * p1.y + 3f * (1 - t) * Mathf.Pow(t, 2f) * p2.y + 3 * Mathf.Pow(t, 3f) * p3.y;
        //ret.z = Mathf.Pow(1 - t, 3f) * p0.z + 3f * Mathf.Pow(1 - t, 2f) * t * p1.z + 3f * (1 - t) * Mathf.Pow(t, 2f) * p2.z + 3 * Mathf.Pow(t, 3f) * p3.z;

        return Mathf.Pow(1 - t, 3f) * p0 + 3f * Mathf.Pow(1 - t, 2f) * t * p1 + 3f * (1 - t) * Mathf.Pow(t, 2f) * p2 + Mathf.Pow(t, 3f) * p3;
        //return ret;
    }
	// Use this for initialization
	void Start () {
        positions = new Vector3[targets.Length];
        four = new Vector3[4];
    }

    public float smoothness = 1f;
    public int steps = 100;
	// Update is called once per frame
	void Update () {

        for(int i = 0; i < targets.Length; ++i) {
            positions[i] = targets[i].position;
        }
        
        Belt.setPath(positions, smoothness);

    }
    
}
