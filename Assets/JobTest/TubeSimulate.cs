//#define DEBUG_JOB
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using Unity.Jobs;
using Unity.Collections;

public class TubeSimulate : MonoBehaviour {
    public static TubeSimulate self;
    public static GenericJob[] generic;
    public int arrayLength = 10000;
    int tubeCount;
    TubeData[] tubes;
    public NativeArray<TubeUpdateData> tubeUpdateData;
    TubeUpdateJob updateTubesJob;
    JobHandle updateTubesJH;
    public NativeArray<byte> endStates;
    public NativeArray<byte> tempOps;

    public int outputStateCount;
    public NativeArray<byte> gOutputStates;

    int generatorCount;
    GeneratorData[] generators;
    
    private void Awake()
    {
        self = this;
        generic = new GenericJob[2];
        generic[0] = new GenericJob();
        generic[1] = new GenericJob();
    }
    // Use this for initialization
    void Start () {
        
        tubes = new TubeData[arrayLength];
        tubeUpdateData = new NativeArray<TubeUpdateData>(arrayLength, Allocator.Persistent);
        endStates = new NativeArray<byte>(arrayLength, Allocator.Persistent);
        tempOps = new NativeArray<byte>(arrayLength, Allocator.Persistent);
        gOutputStates = new NativeArray<byte>(arrayLength, Allocator.Persistent);
        outputStateCount = 0;
        for (int i = 0; i < arrayLength; ++i)
        {
            tubes[i].init(10.0f, 2.0f);
            tubes[i].idxInUpdateArray = i;
            tubes[i].push();
            endStates[i] = 0;
        }
    }
    public void op1Generator(int idx) {
        generators[idx].onExpended();
    }
    public void pushToTube(int idx) {
        tubes[idx].push();
    }
    public bool tubeHasSpace(int idx) {
        return tubes[idx].hasSpace();
    }
    public void addTube(float _length, float _speed)
    {
        if(tubeCount >= arrayLength)
        {
            Debug.Log("max tube exceeded!");
            return;
        }
        tubes[tubeCount].init(_length, _speed);
        tubes[tubeCount].idxInUpdateArray = tubeCount;
        endStates[tubeCount] = 0;

        tubeCount++;
    }
    
    public int addGenerator()
    {
        int ret = generatorCount;
        generators[generatorCount].init(1, 2f, 9999, generatorCount, outputStateCount);
        generatorCount++;
        outputStateCount++;
        return ret;
    }
	public void link(int idx, int val)
	{
        // assume we have unified all 3 types, tubes, generators and converters.
        // we have one array to store the end states(output spot) of all entities.


	}
    public void addConverter()
    {

    }
    public void addConsumer()
    {

    }
    private void OnDestroy()
    {
        gOutputStates.Dispose();
        tempOps.Dispose();
        endStates.Dispose();
        tubeUpdateData.Dispose();
        generic[0].Dispose();
        generic[1].Dispose();
    }
    
    void initGenerators()
    {
        generators = new GeneratorData[arrayLength];
    }
    
    float elapsedTime;
    bool pushed;
    public bool addnew;
    public int currentIndex0;
    public int count0;
    
    void Update() {

        if (addnew)
        {
            addnew = false;
            for (int i = 0; i < arrayLength; ++i)
            {
                tubes[i].push();
            }
        }
        elapsedTime += Time.deltaTime;
        if(elapsedTime > 2.0f && pushed == false)
        {
            pushed = true;
            for (int i = 0; i < arrayLength; ++i)
            {
                tubes[i].push();
            }
        }
        updateTubesJob = new TubeUpdateJob()
        {
            deltaTime = Time.deltaTime,
            dataArray = tubeUpdateData,
            outputOps = tempOps,
        };
#if DEBUG_JOB
        updateJob.Run(objCount);
#else
        updateTubesJH = updateTubesJob.Schedule(arrayLength, 64);
#endif
    }

    private void LateUpdate()
    {
#if DEBUG_JOB
#else
        updateTubesJH.Complete();
#endif
        for (int i = 0; i < arrayLength; ++i)
        {
            int op = updateTubesJob.outputOps[i];
            if (op != 0)
            {
                byte state = endStates[i];
                if (state == 0)
                {
                    // the end point is empty, let the tube remove this element and assign a new one.
                    tubes[i].pop();
                }
                else if (state == 1)
                {
                    // this element is blocked, let the tube update the element before this.
                    tubes[i].saturate();
                }
            }
        }
        // debug
        for (int j = 0; j < arrayLength; ++j)
        {
            TubeData d = tubes[j];
            float offset = d.getOffset();
            for (int i = 0; i < d.count; ++i)
            {
                if (i > d.currentIndex)
                {
                    Debug.DrawLine(new Vector3(d.positions[i], 0f, j), new Vector3(d.positions[i], 1.0f, j), Color.green);
                }
                else
                {
                    Debug.DrawLine(new Vector3(d.positions[i] + offset, 0f, j), new Vector3(d.positions[i] + offset, 1.0f, j), Color.green);
                }
            }
        }
        currentIndex0 = tubes[0].currentIndex;
        count0 = tubes[0].count;

        endState0 = (int)endStates[0];
        if (toggleEndState)
        {
            toggleEndState = false;
            if(endState0 == 0)
            {
                endStates[0] = 1;
            }
            else
            {
                endStates[0] = 0;
                tubes[0].onUnblocked();
            }
        }
        dbgArray = tubes[0].positions;
    }
    public float[] dbgArray;
    public bool toggleEndState;
    public int endState0;
    
    
}
public struct TubeUpdateData
{
    public float speed;
    public float current; // the absolute position of the element being updated along the tube. ranges from 0 to length of TubeData.
    public float boundary; // an ever decreasing value. when it reaches zero, things happen!
    public TubeUpdateData(float _speed)
    {
        speed = _speed;
        boundary = 0.0f;
        current = 0.0f;
    }
}
public struct TubeUpdateJob : IJobParallelFor
{
    [ReadOnly]
    public float deltaTime;
    [WriteOnly]
    public NativeArray<byte> outputOps;
    public NativeArray<TubeUpdateData> dataArray;

    public void Execute(int index)
    {
        TubeUpdateData thisData = dataArray[index];
        byte op = 0;
        if (thisData.speed > 0.0f)
        {
            float movedDist = deltaTime * thisData.speed;
            thisData.current += movedDist;
            dataArray[index] = thisData;
            op = (thisData.current >= thisData.boundary) ? (byte)1 : (byte)0;
        }
        outputOps[index] = op;
    }
}
