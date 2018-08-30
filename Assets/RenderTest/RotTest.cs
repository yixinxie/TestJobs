using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotTest : MonoBehaviour {
    public float radius = 2f;
    float angle;
    public float speed = 270f;
    public Transform center;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 centerpos = center.position;
        angle += Time.deltaTime * speed;
        float rad = angle * Mathf.Deg2Rad;
        Vector3 newpos;
        newpos.x = centerpos.x + radius * Mathf.Cos(rad);
        newpos.z = centerpos.z + radius * Mathf.Sin(rad);
        newpos.y = centerpos.y;
        transform.position = newpos;
    }
}
