using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
public interface IDistanceListener {
    void inRange(IDistanceListener other);
    void outRange(IDistanceListener other);
    void NotifyIndexChange(int new_idx);
}
public struct FloatArray {
    public Vector3 pos;
}
public struct DistanceJob : IJobParallelFor {
    [ReadOnly]
    public Vector3 againstPos;
    public float threshold;
    public int[] contactList;
    public NativeArray<Vector3> positions;
    [WriteOnly]
    public NativeArray<byte> outputOps;
    //public NativeArray<GenericUpdateData> dataArray;

    public void Execute(int index) {
        Vector3 cmp = positions[index];
        float distance = Vector3.Distance(againstPos, cmp);
        if(distance < threshold) {
            outputOps[index] = 1;
        }
        
    }
}
public class DynamicDistanceCheck : MonoBehaviour {
    public static DynamicDistanceCheck self;
    const int defaultLength = 5;
    int beginAt;
    const int maxCountPerFrame = 5;
    
    List<int> contactList = new List<int>(8);
    // Use this for initialization
    private void Awake() {
        self = this;
    }
    private void Start() {
    }
    private void OnDestroy() {
    }
    public int Add(Vector3 pos, float threshold, IDistanceListener agent) {
        int ret = map.Add(pos, threshold * threshold, agent);
        return ret;
    }
    public void updatePosition(int idx, Vector3 newPos) {
        map.arOne[idx] = newPos;
    }

    public void updateThreshold(int idx, float newVal) {
        map.arTwo[idx] = newVal * newVal;
    }
    public void Remove(IDistanceListener agent) {
        int idx = map.LinearFindThird(agent);
        if(idx >= 0) {
            int notify = map.count - 1;
            removeFromContactList(idx);
            map.arThree[notify].NotifyIndexChange(idx);
            changeIndexInContact(notify, idx);
            map.Remove(idx);
            
        }
    }
    void removeFromContactList(int toRemove) {
        for (int i = contactList.Count - 1; i >= 0; --i) {
            int val = contactList[i];
            int high = val >> hash_shift;
            int low = val & ((1 << hash_shift) - 1);
            if (high == toRemove) {
                contactList.RemoveAt(i);
            }
            if (low == toRemove) {
                contactList.RemoveAt(i);
                map.arThree[high].outRange(map.arThree[low]);
            }
        }
    }
    void changeIndexInContact(int old_idx, int new_idx) {
        for (int i = contactList.Count - 1; i >= 0; --i) {
            int val = contactList[i];
            int high = val >> hash_shift;
            int low = val & ((1 << hash_shift) - 1);
            if (high == old_idx) {
                high = new_idx;
            }
            if (low == old_idx) {
                low = new_idx;
            }
            contactList[i] = (high << hash_shift) + low;
        }
    }
    private void Update() {
    }
    const int hash_shift = 12;
    // Update is called once per frame
    private void update() {
        Vector3[] positionArray = map.arOne;
        int itemCount = map.count;
        //countPerFrame = itemCount;
        int countPerFrame = Mathf.Min(maxCountPerFrame, itemCount);
        int i = beginAt;
        if (i >= itemCount) i = 0; // this can happen if an agent is removed between two updates.
        for (int inc = 0; inc < countPerFrame; ++inc) {
            i = beginAt + inc;
            i %= itemCount;
            Vector3 againstPos = positionArray[i];
            float distanceThreshold = map.arTwo[i];
            for (int j = 0; j < itemCount; ++j) {
                if (i == j) continue; // no self-check
                float dist = (againstPos - positionArray[j]).sqrMagnitude;
                int key = (i << hash_shift) + j;
                bool contain = contactList.Contains(key);
                if (dist < distanceThreshold && contain == false) {
                    map.arThree[i].inRange(map.arThree[j]);
                    contactList.Add(key);
                }
                else if (dist >= distanceThreshold && contain) {
                    map.arThree[i].outRange(map.arThree[j]);
                    contactList.Remove(key);
                }
            }
        }
        beginAt += countPerFrame;
    }
}
