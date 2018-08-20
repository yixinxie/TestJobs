using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Simulation_OOP;
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
    public Vector3 pointedAt;
	// Update is called once per frame
	void Update () {
	    if(Input.touchCount == 1) {
            Vector2 deltaPos = Input.touches[0].deltaPosition;
            Vector3 up = Vector3.Cross(Vector3.down, cam.transform.right);
            Vector3 camDelta = deltaPos.x * -cam.transform.right / cam.aspect + deltaPos.y * -up;
            cam.transform.position += camDelta / cam.orthographicSize;
            
        }
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool rayhit = Physics.Raycast(ray, out hit);
        if (rayhit) {
            pointedAt = hit.point;
            pointedAt.x = Mathf.RoundToInt(pointedAt.x);
            pointedAt.z = Mathf.RoundToInt(pointedAt.z);
            pointedAt.y = 0f;
        }
        if (Input.GetMouseButtonUp(0)) {
            byte buildPhase = UIControll.self.getBuildPhase();
            if (buildPhase == 2) {
                bool buildResult = true;
                //Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                //RaycastHit hit;
                if (rayhit) {
                    int idx = UIControll.self.getBuildStructure();
                    switch (idx) {
                        case 0:
                            buildResult = SimManager.self.addGenerator(pointedAt) != null;
                            break;
                        case 1:
                            SimManager.self.addInserter(pointedAt);
                            break;
                        case 2:
                            //SimManager.self.addBelt(pointedAt);
                            break;
                        case 3:
                            SimManager.self.addAssembler(pointedAt);
                            break;
                        case 4:
                            SimManager.self.addStorage(pointedAt);
                            break;
                    }
                }
                if (buildResult) {
                    UIControll.self.resetBuildEvent();
                }
            }
        }
        //if (Input.GetMouseButtonDown(0)) 
            {
            byte buildPhase = UIControll.self.getBuildPhase();
            if(buildPhase == 3) {
                //Debug.Log("belt mode");
                if (Input.GetMouseButtonDown(0)) {
                    beltStart = pointedAt;
                    settingBelt = true;
                    Debug.Log("belt begin " + Input.mousePosition);
                }
                else if(Input.GetMouseButtonUp(0) && settingBelt) {
                    Vector3 beltEnd = pointedAt;
                    SimManager.self.addBelt(beltStart, beltEnd);
                    settingBelt = false;
                    Debug.Log("belt end " + Input.mousePosition);
                }
            }
        }
	}
    bool settingBelt;
    Vector3 beltStart;
}
