using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct PositionInfo {
    public Vector3 pos;
    public int entity;
}
public class NewBehaviourScript : MonoBehaviour {
    List<List<PositionInfo>> map;
    public int cellCount = 10;
    public float cellSize = 10f;
    public int totalCount;
    List<Transform> targets;
    // Use this for initialization
    private void Awake() {
        totalCount = cellCount * cellCount;
        map = new List<List<PositionInfo>>(totalCount);
        for(int i = 0; i < totalCount; ++i) {
            map.Add(new List<PositionInfo>());
        }
        targets = new List<Transform>();
    }
    void rehash() {
        
        for (int i = 0; i < totalCount; ++i) {
            map[i].Clear();
        }

    }
    int getHashes(Vector3 pos, float radius) {
        float left = pos.x - radius;
        float top = pos.z - radius;

        int intx = Mathf.RoundToInt(left / cellSize);
        int intz= Mathf.RoundToInt(top / cellSize);
        rehashRet[0] = intx + intz * cellCount;

        intx = Mathf.RoundToInt((left + 2f * radius) / cellSize);
        intz = Mathf.RoundToInt(top / cellSize);
        rehashRet[1] = intx + intz * cellCount;

        intx = Mathf.RoundToInt(left / cellSize);
        intz = Mathf.RoundToInt((top + 2f * radius) / cellSize);
        rehashRet[2] = intx + intz * cellCount;

        intx = Mathf.RoundToInt((left + 2f * radius) / cellSize);
        intz = Mathf.RoundToInt((top + 2f * radius) / cellSize);
        rehashRet[3] = intx + intz * cellCount;
        return 0;
    }
    int[] rehashRet = new int[8];


    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
