using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestIAgentScene : MonoBehaviour {
    public int count = 1000;
    public float radius = 100f;
    public Object prefab;
	// Use this for initialization
	void Start () {
		for(int i = 0; i < count; ++i) {
            GameObject go = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            go.transform.position = new Vector3(Random.Range(-radius, radius), 0f, Random.Range(-radius, radius));
            TestIAgent agent = go.GetComponent<TestIAgent>();
            Pathea.HotAreaNs.HotAreaModule.self.Add(agent);
        }
	}
}
