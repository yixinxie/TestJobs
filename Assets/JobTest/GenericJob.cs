//#define DEBUG_JOB
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
    //CanTakeState[] endStates;
    // Use this for initialization
    public GenericJob() {
        genericUpdateData = new NativeArray<GenericUpdateData>(arrayLength, Allocator.Persistent);
        tempGenericUpdateOps = new NativeArray<byte>(arrayLength, Allocator.Persistent);
        count = 0;
        //endStates = new CanTakeState[arrayLength];
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
#if DEBUG_JOB
        genericUpdateJob.Run(objCount);
#else
        genericUpdateJH = genericUpdateJob.Schedule(count, 64);
#endif
    }

    public void lateUpdate() {
#if DEBUG_JOB
#else
        genericUpdateJH.Complete();
#endif
        //for(int i = 0; i < tempGenericUpdateOps.Length; ++i) {
        //    if(tempGenericUpdateOps[i] != 0) {
        //        // check if this generator has space at its end?
        //        if (TubeSimulate.self.tubeHasSpace(endStates[i].idx)) {
                    
        //            TubeSimulate.self.pushToTube(endStates[i].idx);
        //            // consume a unit of resource.
        //            TubeSimulate.self.op1Generator(i);
        //        }
        //    }
        //}
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
        if (thisData.timeLeft < 0.0f) {
            outputOps[index] = 0;
        }
        else {
            //thisData.timeLeft -= deltaTime * thisData.speed;
            thisData.timeLeft -= deltaTime;

            dataArray[index] = thisData;
            outputOps[index] = (thisData.timeLeft <= 0.0f) ? (byte)1 : (byte)0;
        }
    }
}