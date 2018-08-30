using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotTest : MonoBehaviour {
    float radius = 2f;
    float angle;
    public float speed = 90f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        angle += Time.deltaTime * speed;
        float rad = angle * Mathf.Deg2Rad;
        Vector3 newpos;
        newpos.x = radius * Mathf.Cos(rad);
        newpos.z = radius * Mathf.Sin(rad);
        newpos.y = 0f;
        transform.position = newpos;
    }
}
