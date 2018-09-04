using Pathea.HotAreaNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestIAgent : MonoBehaviour, IAgent {
    public float distanceThreshold = 5f;
    Vector3 cachedPos;
    
    public void Distance(float distance) {

        Debug.DrawLine(cachedPos, cachedPos + Vector3.up * 3f, Color.green, 3f);

    }

    public float GetDistanceThreshold() {
        return distanceThreshold;
    }

    public float GetInterval() {
        return 3f;
    }

    public Vector3 GetPos() {
        return cachedPos;
    }
    // Use this for initialization
    void Start () {
        cachedPos = transform.position;
    }
}
