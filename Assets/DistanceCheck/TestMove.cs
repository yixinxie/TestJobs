using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMove : MonoBehaviour, IDistanceListener {

	// Use this for initialization
	void Start () {
        distanceHandle = DynamicDistanceCheck.self.Add(transform.localPosition, 5f, this);
        TestMoveManager.self.Add(this);
	}
    float phaseDurationLeft = 0f;
    Vector3 currentDir;
    const float speed = 3f;
    int distanceHandle;
    // Update is called once per frame
    public void update (float dt) {
        Vector3 pos = transform.localPosition;
        pos += currentDir * speed * dt;
        transform.localPosition = pos;
        phaseDurationLeft -= dt;
        if (phaseDurationLeft <= 0.0f) {
            phaseDurationLeft = Random.Range(3f, 5f);
            float radian = Random.Range(0f, Mathf.PI * 2f);
            currentDir = new Vector3(Mathf.Cos(radian), 0f, Mathf.Sin(radian));
        }
        DynamicDistanceCheck.self.updatePosition(distanceHandle, pos);
        Vector3 local = transform.localPosition;
        for(int i = 0; i < adj.Count; ++i) {
            Vector3 toPos = adj[i].localPosition;
            Debug.DrawLine(local, toPos, Color.green);
        }
    }
    List<Transform> adj = new List<Transform>();
    void OnDestroy() {
        DynamicDistanceCheck.self.Remove(this);
        TestMoveManager.self.Remove(this);
    }

    public void inRange(IDistanceListener other) {
        MonoBehaviour otherMove = other as MonoBehaviour;
        
        adj.Add(otherMove.transform);

    }
    public void outRange(IDistanceListener other) {
        MonoBehaviour otherMove = other as MonoBehaviour;

        adj.Remove(otherMove.transform);
    }

    public void NotifyIndexChange(int new_idx) {
        distanceHandle = new_idx;
    }
}
