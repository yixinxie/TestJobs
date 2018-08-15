using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRail : MonoBehaviour {
    public Vector3 from;
    public Vector3 to;
    float lerpDir;
    float alpha;
    public float speed = 5f;
	// Use this for initialization
	void Start () {
        lerpDir = 1f;
        from = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        alpha += Time.deltaTime / speed * lerpDir;
            
        if(alpha > 1f) {
            alpha = 1f;
            lerpDir = -1f;
        }
        else if (alpha < 0f) {
            alpha = 0f;
            lerpDir = 1f;
        }
        transform.position = Vector3.Lerp(from, to, alpha);
    }
}
