//#define DEBUG_JOB
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Profiling;

public struct CanTakeState {
    public int idx;
    public byte cachedState;
    public byte type;
}
public class TubeSimulate : MonoBehaviour {
    [HideInInspector]
    public static TubeSimulate self;
    [HideInInspector]
    public static GenericJob[] generic;
    public int arrayLength = 10000;
    int tubeCount;
    TubeData[] tubes;
    [HideInInspector]
    public NativeArray<TubeUpdateData> tubeUpdateData;
    TubeUpdateJob updateTubesJob;
    JobHandle updateTubesJH;
    [HideInInspector]
    public CanTakeState[] endStates;
    [HideInInspector]
    public NativeArray<byte> tempOps;

    public int outputStateCount;
    //public NativeArray<byte> gOutputStates;

    List<GeneratorData> generators;
    
    private void Awake()
    {
        self = this;
        generic = new GenericJob[2];
        generic[0] = new GenericJob();
        generic[1] = new GenericJob();
    }
    CustomSampler sampler;
    // Use this for initialization
    void Start () {
        sampler = UnityEngine.Profiling.CustomSampler.Create("Job System");
        tubes = new TubeData[arrayLength];
        tubeUpdateData = new NativeArray<TubeUpdateData>(arrayLength, Allocator.Persistent);
        //endStates = new NativeArray<CanTakeState>(arrayLength, Allocator.Persistent);
        endStates = new CanTakeState[arrayLength];
        tempOps = new NativeArray<byte>(arrayLength, Allocator.Persistent);
        //gOutputStates = new NativeArray<byte>(arrayLength, Allocator.Persistent);
        outputStateCount = 0;

        generators = new List<GeneratorData>();
        addGenerator();
        for (int i = 0; i < arrayLength; ++i)
        {
            tubes[i].init(10.0f, 2.0f);
            tubes[i].idxInUpdateArray = i;
            //tubes[i].push();
            tubes[i].idxInStateArray = registerEndState();
            endStates[i].cachedState = 0;
            if(i == 0)
            {
                linkTubeToGenerator(0, i);
            }
            else
            {
                linkTubeToTube(i - 1, i);
            }
        }
        
        //tubes[0].push();
    }
    void linkTubeToGenerator(int generatorIdx, int tubeIdx) // generator ==> tube
    {
        GeneratorData genData = generators[generatorIdx];
        CanTakeState cts = endStates[genData.idxInOutputStateArray];
        cts.type = 0;
        cts.idx = tubeIdx;
        endStates[genData.idxInOutputStateArray] = cts;
    }

    void linkTubeToTube(int tubeIdxPre, int tube1IdxSucc) // tube ==> tube
    {
        TubeData tubeData = tubes[tubeIdxPre];
        CanTakeState cts = endStates[tubeData.idxInStateArray];
        cts.type = 0;
        cts.idx = tube1IdxSucc;
        endStates[tubeData.idxInStateArray] = cts;
    }
    // tube to converter
    // consumer to tube

    public void pushToTube(int idx) {
        tubes[idx].push();
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
        tubeCount++;
    }
    
    public int addGenerator()
    {
        // when we create a new generator, it should claim a space in the endStates array as indicated by the index 'generators[i].idxInOutputStateArray'.
        int ret = generators.Count;
        generators.Add(new GeneratorData());
        GeneratorData gen = generators[ret];
        gen.init(1, 2f, 3, ret, registerEndState());
        gen.start();
        generators[ret] = gen;
        return ret;
    }
    public int registerEndState() {
        int ret = outputStateCount;
        outputStateCount++;
        return ret;
    }
    public void addConverter()
    {

    }
    public void addConsumer()
    {

    }
    private void OnDestroy()
    {
        //gOutputStates.Dispose();
        tempOps.Dispose();
        //endStates.Dispose();
        tubeUpdateData.Dispose();
        generic[0].Dispose();
        generic[1].Dispose();
    }
    
    float elapsedTime;
    bool pushed;
    public bool addnew;
    public int currentIndex0;
    public int count0;
    
    void Update() {
        sampler.Begin();
        if (addnew)
        {
            addnew = false;
            //for (int i = 0; i < arrayLength; ++i)
            //{
            //    tubes[i].push();
            //}
            tubes[0].push();
        }
        float deltaTime = Time.deltaTime;
        elapsedTime += deltaTime;
        generic[0].update(deltaTime);
        generic[1].update(deltaTime);
        if (elapsedTime > 2.0f && pushed == false)
        {
            pushed = true;
            //for (int i = 0; i < arrayLength; ++i)
            {
                //tubes[i].push();
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
        gener
#else
        updateTubesJH = updateTubesJob.Schedule(arrayLength, 64);
#endif
        sampler.End();
    }

    private void LateUpdate()
    {
        sampler.Begin();
        generic[0].lateUpdate();
        

        generic[1].lateUpdate();
#if DEBUG_JOB
#else
        updateTubesJH.Complete();
#endif
        // tubes output
        for (int i = 0; i < arrayLength; ++i)
        {
            int op = updateTubesJob.outputOps[i];
            if (op != 0)
            {
                CanTakeState cts = endStates[tubes[i].idxInStateArray];
                if (cts.type == 0) // tube
                {
                    //if (cts.cachedState == 0)
                    //{
                    //    // the end point is empty, let the tube remove this element and assign a new one.
                    //    tubes[i].pop();
                    //    tubes[cts.idx].push();
                    //}
                    //else if (cts.cachedState == 1)
                    //{
                    //    // this element is blocked, let the tube update the element before this.
                    //    tubes[i].saturate();
                    //}

                    if (tubes[cts.idx].hasSpace())
                    {
                        tubes[i].pop();
                        tubes[cts.idx].push();
                        // consume a unit of resource.
                    }
                    else
                    {
                        tubes[i].saturate();
                    }
                }
                else if(cts.type == 1)
                {

                }
            }
        }
        
        // generator output
        for (int i = 0; i < generic[0].tempGenericUpdateOps.Length; ++i)
        {
            if (generic[0].tempGenericUpdateOps[i] != 0)
            {
                CanTakeState cts = endStates[generators[i].idxInOutputStateArray];
                // check if this generator has space at its end?
                if (tubes[cts.idx].hasSpace())
                {
                    tubes[cts.idx].push();
                    // consume a unit of resource.
                    GeneratorData gen = generators[i];
                    gen.onExpended();
                    generators[i] = gen;
                }
            }
        }
        sampler.End();
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

        //endState0 = (int)endStates[0];
        //if (toggleEndState)
        //{
        //    toggleEndState = false;
        //    if(endState0 == 0)
        //    {
        //        endStates[0] = 1;
        //    }
        //    else
        //    {
        //        endStates[0] = 0;
        //        tubes[0].onUnblocked();
        //    }
        //}
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
