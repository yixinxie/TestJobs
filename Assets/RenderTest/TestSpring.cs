using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
public struct ClothPointData {
    public Vector3 position;
    public Vector3 velocity;
    public float length;
    public Vector3 constraintDirection;
    public Vector3 occDirection;
}
// targets are transforms
public class TestSpring : MonoBehaviour {
    public Transform[] targets;
    public Transform rootTrans;
    ClothPointData[] points;
    public Vector3 colliderSphereCenter;
    public float colliderSphereRadius;

    public float dragCoeff = 0.5f;
    public float _stretchLength = 1f;
    public float stiffness = 1f;
    Vector3 lastFrameBase;
    // Use this for initialization
    private void Awake() {
        points = new ClothPointData[targets.Length];
        
    }
    void Start () {
        Vector3 parentPos = rootTrans.position;
        colliderSphereCenter -= parentPos;
        for (int i = 0; i < targets.Length; ++i) {
            points[i].position = targets[i].position;
            points[i].length = Vector3.Distance(points[i].position, parentPos);
            parentPos = points[i].position;
        }
	}

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(colliderSphereCenter + rootTrans.position, colliderSphereRadius);
    }
    public float floatCoeff = 10f;
    void Update() {

        //Thread thr = new Thread();
        float maxSpring = 0.5f;
        float minSpring = 0.1f;
        Vector3 gravity = Physics.gravity;
        float stretchLength = _stretchLength;
        float dt = Time.deltaTime;
        Vector3 parentPos = rootTrans.position;
        Vector3 basePos = parentPos;
        //float stiffness = 1f;
        for (int i = 0; i < points.Length; ++i) {
            ClothPointData pd = points[i];
            Vector3 worldPos = pd.position;
            Vector3 acceleration = gravity;
            pd.occDirection = Vector3.zero;

            float maxDist = pd.length * 1.2f;
            float minDist = pd.length * 0.8f;
            float dist = Vector3.Distance(worldPos, parentPos);
            Vector3 diff = worldPos - parentPos;
            diff /= dist;
            if (dist > pd.length) {
                float exceed = Mathf.Clamp01(Mathf.Abs(dist - maxDist) / stretchLength);
                worldPos = parentPos + diff * Mathf.Lerp(dist, maxDist, exceed * exceed);
                pd.constraintDirection = -diff * Mathf.Clamp01(0.2f + exceed);

                float springMag = maxSpring * exceed * 9.8f;
                Vector3 force = -diff * springMag;
                acceleration += force;

            }
            else {
                float exceed = Mathf.Clamp01(Mathf.Abs(dist - minDist) / stretchLength);
                worldPos = parentPos + diff * Mathf.Lerp(dist, minDist, exceed * exceed);
                pd.constraintDirection = diff * Mathf.Clamp01(0.2f + exceed);

                float springMag = minSpring * exceed * 9.8f;
                Vector3 force = -diff * springMag;
                acceleration += force;
            }
            acceleration += -pd.velocity * dragCoeff;
            // collision check
            Vector3 floatDiff = worldPos - colliderSphereCenter + basePos;
            float floatDiffMag = floatDiff.magnitude;
            if(floatDiffMag < colliderSphereRadius) {
                acceleration += floatCoeff * floatDiff / floatDiffMag * colliderSphereRadius / floatDiffMag;
                //Debug.Log(i);
            }

            // velocity calculation and application
            
            //acceleration += pd.constraintDirection * Mathf.Max(0f, -Vector3.Dot(pd.constraintDirection, acceleration));
            //acceleration += pd.occDirection * Mathf.Max(0f, -Vector3.Dot(pd.occDirection, acceleration));

            pd.velocity += acceleration * dt;

            //Vector3 ncd = pd.constraintDirection.normalized;
            //float lcd = pd.constraintDirection.magnitude;
            //Vector3 friction = (pd.velocity + ncd * Mathf.Max(0f, -Vector3.Dot(ncd, pd.velocity))) * lcd * 0.5f;

            //pd.velocity += pd.constraintDirection * Mathf.Max(0f, -Vector3.Dot(pd.constraintDirection, pd.velocity)) - friction * stiffness * lcd;
            //float occ = Mathf.Max(0f, -Vector3.Dot(pd.occDirection, pd.velocity));
            //pd.velocity += pd.occDirection * occ;
            //pd.velocity *= (1 - Mathf.Pow(occ, 0.3f) * 0.1f);


            worldPos += pd.velocity * dt;

            parentPos = worldPos;
            pd.position = worldPos;
            points[i] = pd;
        }
        //lastFrameBase = basePos;
        
        for (int i = 0; i < points.Length; ++i) {
            targets[i].localPosition = points[i].position - basePos;
        }
    }
    
    private void LateUpdate() {
        //Vector3 parentPos = Vector3.zero;
        Vector3 basePos = rootTrans.position;
        //Vector3 parentPos = Vector3.zero;
        Vector3 parentPos = rootTrans.position;
        Color[] clrs = new Color[3] { Color.green, Color.red, Color.blue};
        for (int i = 0; i < points.Length; ++i) {
            //Debug.DrawLine(parentPos + lastFrameBase, points[i].position + lastFrameBase, clrs[i % 3]);
            Debug.DrawLine(parentPos, points[i].position, clrs[i % 3]);
            parentPos = points[i].position;
            //targets[i].localPosition = points[i].position;
        }
    }
}
