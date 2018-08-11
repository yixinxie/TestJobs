using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour {
    public static CamControl self;
    public Camera cam;
    private void Awake() {
        self = this;
        cam = GetComponent<Camera>();
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.touchCount == 1) {
            Vector2 deltaPos = Input.touches[0].deltaPosition;
            Vector3 up = Vector3.Cross(Vector3.down, cam.transform.right);
            Vector3 camDelta = deltaPos.x * -cam.transform.right / cam.aspect + deltaPos.y * -up;
            cam.transform.position += camDelta / cam.orthographicSize;
            
        }
	}
}
