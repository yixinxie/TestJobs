using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct ClothPointData {
    public Vector3 position;
    public Vector3 velocity;
    public float length;
}
public class TestSpring : MonoBehaviour {
    public Transform[] targets;
    public Transform rootTrans;
    ClothPointData[] points;

    public float adjustSpeed = 10f;
    public float dragCoeff = 0.5f;

    // Use this for initialization
    private void Awake() {
        points = new ClothPointData[targets.Length];
        
    }
    void Start () {
        Vector3 parentPos = rootTrans.position;
		for(int i = 0; i < targets.Length; ++i) {
            points[i].position = targets[i].position;
            points[i].length = Vector3.Distance(points[i].position, parentPos);
            parentPos = points[i].position;

        }
	}
    
	// Update is called once per frame
	void Update () {
        float dt = Time.deltaTime;
        Vector3 parentPos = rootTrans.position;
        for (int i = 0;i < points.Length; ++i) {
            ClothPointData pd = points[i];
            float dist = Vector3.Distance(pd.position, parentPos);
            //pd.length
            Vector3 diff = pd.position - parentPos;
            diff.Normalize();
            Vector3 desiredPos = parentPos + pd.length * diff;
            Vector3 moveDir = desiredPos - pd.position;
            float moveDirMag = moveDir.magnitude;
            if (moveDirMag != 0)
                moveDir /= moveDirMag;
            pd.velocity += moveDir * dt * moveDirMag * adjustSpeed;

            // gravity
            pd.velocity += Vector3.down * dt;

            // drag
            pd.velocity *= (1f - dragCoeff * dt);
            pd.position += pd.velocity * dt;
            points[i] = pd;
            //diff * 
            parentPos = pd.position;
        }
	}
    private void LateUpdate() {
        Vector3 parentPos = rootTrans.position;
        Color[] clrs = new Color[3] { Color.green, Color.red, Color.blue};
        for (int i = 0; i < points.Length; ++i) {
            Debug.DrawLine(parentPos, points[i].position, clrs[i % 3]);
            parentPos = points[i].position;
            targets[i].localPosition = points[i].position;
        }
    }
}
