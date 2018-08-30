using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
public struct ClothPointData {
    public Vector3 position;
    public Vector3 velocity;
    public float length;
    //public Vector3 constraintDirection;
    //public Vector3 occDirection;
}
public class TestSpring : MonoBehaviour {
    public Transform[] targets;
    public Transform rootTrans;
    ClothPointData[] points;

    public float dragCoeff = 0.5f;
    public float _stretchLength;
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
    
    
    void Update() {

        //Thread thr = new Thread();
        float maxSpring = 0.5f;
        float minSpring = 0.1f;
        Vector3 gravity = Physics.gravity;
        float stretchLength = _stretchLength;
        float dt = Time.deltaTime;
        Vector3 parentPos = rootTrans.position;
        //Vector3 parentPos = Vector3.zero;
        //float stiffness = 1f;
        for (int i = 0; i < points.Length; ++i) {
            ClothPointData pd = points[i];
            Vector3 acceleration = gravity;

            float maxDist = pd.length * 1.2f;
            float minDist = pd.length * 0.8f;
            float dist = Vector3.Distance(pd.position, parentPos);
            Vector3 diff = pd.position - parentPos;
            //diff.Normalize();
            diff /= dist;
            if (dist > pd.length) {
                float exceed = Mathf.Clamp01(Mathf.Abs(dist - maxDist) / stretchLength);
                pd.position = parentPos + diff * Mathf.Lerp(dist, maxDist, exceed * exceed);

                float springMag = maxSpring * exceed * 9.8f;
                Vector3 force = -diff * springMag;
                acceleration += force;

            }
            else {
                float exceed = Mathf.Clamp01(Mathf.Abs(dist - minDist) / stretchLength);
                pd.position = parentPos + diff * Mathf.Lerp(dist, minDist, exceed * exceed);

                float springMag = minSpring * exceed * 9.8f;
                Vector3 force = -diff * springMag;
                acceleration += force;
            }

            acceleration += -pd.velocity * dragCoeff;

            pd.velocity += acceleration * dt;
            pd.position += pd.velocity * dt;

            points[i] = pd;
            parentPos = pd.position;
        }
    }
    
    private void LateUpdate() {
        //Vector3 parentPos = Vector3.zero;
        //Vector3 basePos = rootTrans.position;
        //Vector3 parentPos = Vector3.zero;
        Vector3 parentPos = rootTrans.position;
        Color[] clrs = new Color[3] { Color.green, Color.red, Color.blue};
        for (int i = 0; i < points.Length; ++i) {
            Debug.DrawLine(parentPos, points[i].position, clrs[i % 3]);
            parentPos = points[i].position;
            targets[i].localPosition = points[i].position;
        }
    }
}
