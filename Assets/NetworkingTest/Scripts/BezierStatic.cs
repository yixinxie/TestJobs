using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierStatic : MonoBehaviour {
    public Transform[] four;
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
		
	}

    public float toVal = 1f;
    public int steps = 100;
	// Update is called once per frame
	void Update () {
        Vector3 lastPos = evalBezier(four[0].position, four[1].position, four[2].position, four[3].position, 0);
        for (int i = 1; i < steps; i++) {
            float t = (toVal / (float)steps) * i;
            Vector3 thisPos = evalBezier(four[0].position, four[1].position, four[2].position, four[3].position, t);
            Debug.DrawLine(lastPos, thisPos, Color.green);
            lastPos = thisPos;

        }
	}
}
