#define DEBUG_JOB
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using Unity.Jobs;
using Unity.Collections;

public class GenericJob {
    public int arrayLength = 10000;
    GenericUpdateJob genericUpdateJob;
    JobHandle genericUpdateJH;
    int generalUpdateCount;
    public NativeArray<GenericUpdateData> genericUpdateData;
    public NativeArray<byte> tempGenericUpdateOps;
    int count;
    bool debug;
    //CanTakeState[] endStates;
    // Use this for initialization
    public GenericJob(bool _debug) {
        genericUpdateData = new NativeArray<GenericUpdateData>(arrayLength, Allocator.Persistent);
        //for(int i =0; i< arrayLength; ++i) {
        //    genericUpdateData[i].timeLeft = -1f;

        //}
        
        tempGenericUpdateOps = new NativeArray<byte>(arrayLength, Allocator.Persistent);
        count = 0;
        debug = _debug;
    }
    public void Dispose()
    {
        tempGenericUpdateOps.Dispose();
        genericUpdateData.Dispose();
    }
    public int addEntity() {
        int ret = count;

        count++;
        return ret;
    }
    
    public void update(float deltaTime) {

        genericUpdateJob = new GenericUpdateJob()
        {
            deltaTime = deltaTime,
            dataArray = genericUpdateData,
            outputOps = tempGenericUpdateOps,
        };
        if (debug) {
            genericUpdateJob.Run(count);
        }
        else {
            genericUpdateJH = genericUpdateJob.Schedule(count, 64);
        }
    }

    public void lateUpdate() {
        if (!debug) {
            genericUpdateJH.Complete();
        }
    }
}
public struct GenericUpdateData {
    public float timeLeft;
}
public struct GenericUpdateJob : IJobParallelFor {
    [ReadOnly]
    public float deltaTime;
    [WriteOnly]
    public NativeArray<byte> outputOps;
    public NativeArray<GenericUpdateData> dataArray;

    public void Execute(int index) {
        GenericUpdateData thisData = dataArray[index];
        if (thisData.timeLeft <= 0.0f) {
            outputOps[index] = 0;
        }
        else {
            thisData.timeLeft -= deltaTime;

            dataArray[index] = thisData;
            outputOps[index] = (thisData.timeLeft <= 0.0f) ? (byte)1 : (byte)0;
        }
    }
}