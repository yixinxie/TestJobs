using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Simulation_OOP;
public class CamControll : MonoBehaviour {
    public static CamControll self;
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
        if (Input.GetMouseButtonUp(0)) {
            if (UIControll.self.isBuildEvent()) {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) {
                    int idx = UIControll.self.getBuildStructure();
                    switch (idx) {
                        case 0:
                            SimManager.self.addGenerator(hit.point);
                            break;
                        case 1:
                            SimManager.self.addInserter(hit.point);
                            break;
                        case 2:
                            SimManager.self.addBelt(hit.point);
                            break;
                        case 3:
                            SimManager.self.addAssembler(hit.point);
                            break;
                        case 4:
                            SimManager.self.addStorage(hit.point);
                            break;
                    }
                }
            }
        }
	}
}
