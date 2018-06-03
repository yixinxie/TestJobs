#define DEBUG_JOB
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

    /* currently two.*/
    [HideInInspector]
    public static GenericJob[] generic;
    public int arrayLength = 10000;

    /* Tube related */
    int tubeCount;
    TubeData[] tubes;
    [HideInInspector]
    public NativeArray<TubeUpdateData> tubeUpdateData;
    [HideInInspector]
    public NativeArray<byte> tempOps;
    TubeUpdateJob updateTubesJob;
    JobHandle updateTubesJH;


    [HideInInspector]
    public CanTakeState[] endStates;
    public int outputStateCount;

    /* generator related */
    List<GeneratorData> generators;

    /* converter related*/
    List<ConverterData> converters;

    /* debug */
    CustomSampler sampler;
    float elapsedTime;
    bool pushed;
    public bool addnew;
    public int currentIndex0;
    public int count0;
    public float[] dbgArray;
    public bool toggleEndState;
    public int endState0;

    private void Awake()
    {
        self = this;

        tubes = new TubeData[arrayLength];
        tubeUpdateData = new NativeArray<TubeUpdateData>(arrayLength, Allocator.Persistent);
        tempOps = new NativeArray<byte>(arrayLength, Allocator.Persistent);

        endStates = new CanTakeState[arrayLength];

        generic = new GenericJob[2];
#if DEBUG_JOB
        generic[0] = new GenericJob(true);
        generic[1] = new GenericJob(true);
#else
        generic[0] = new GenericJob(false);
        generic[1] = new GenericJob(false);
#endif

        generators = new List<GeneratorData>();
        converters = new List<ConverterData>();
    }
    // Use this for initialization
    void Start () {
        sampler = UnityEngine.Profiling.CustomSampler.Create("Job System");
        outputStateCount = 0;
        treeTest(1);
    }
    void simpleTest() {
        addGenerator(1);
        for (int i = 0; i < arrayLength; ++i) {
            tubes[i].init(10.0f, 2.0f);
            tubes[i].idxInUpdateArray = i;
            //tubes[i].push();
            tubes[i].idxInOutputStateArray = registerEndState();
            endStates[i].cachedState = 0;
            if (i == 0) {
                linkTubeToGenerator(0, i);
            }
            else {
                linkTubeToTube(i - 1, i);
            }
        }
    }
    void treeTest(int originCountPower) {
        int generatorCount = 1 << originCountPower;
        int[] ids = new int[generatorCount];
        for (int i = 0; i < generatorCount; ++i) {

            ids[i] = addGenerator((ushort)(i + 1)); 
        }

        List<int> tubeIds = new List<int>();
        for (int i = 0; i < generatorCount; ++i) {
            int tubeId = addTube(5f, 2f);
            tubeIds.Add(tubeId);
            linkTubeToGenerator(ids[i], tubeId);
        }

        List<int> converterIds = new List<int>();
        for (int i = 0; i < generatorCount / 2; ++i) {

            int convId = addConverter();
            converterIds.Add(convId);
            linkConverterToTube(tubeIds[i * 2], convId);
            linkConverterToTube(tubeIds[i * 2 + 1], convId);
        }

        //for (int i = 0; i < generatorCount / 2; ++i) {
        //    int tubeId = addTube(5f, 2f);
        //    tubeIds.Add(tubeId);
        //    linkTubeToConverter(converterIds[i], tubeId);
        //}
    }
    // link consumer to tube
    void linkConverterToTube(int tubeIdx, int converterIdx) // tube ==> converter
    {
        TubeData tube = tubes[tubeIdx];
        CanTakeState cts = endStates[tube.idxInOutputStateArray];
        cts.type = 1;
        cts.idx = converterIdx;
        endStates[tube.idxInOutputStateArray] = cts;
    }
    void linkTubeToConverter(int converterIdx, int tubeIdx) // converter ==> tube
    {
        ConverterData conv = converters[converterIdx];
        CanTakeState cts = endStates[conv.idxInOutputStateArray];
        cts.type = 0;
        cts.idx = tubeIdx;
        endStates[conv.idxInOutputStateArray] = cts;
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
        CanTakeState cts = endStates[tubeData.idxInOutputStateArray];
        cts.type = 0;
        cts.idx = tube1IdxSucc;
        endStates[tubeData.idxInOutputStateArray] = cts;
    }
    // tube to converter
    // consumer to tube

    public int addTube(float _length, float _speed)
    {
        int ret = tubeCount;
        if(tubeCount >= arrayLength)
        {
            Debug.Log("max tube exceeded!");
            return -1;
        }
        tubes[tubeCount].init(_length, _speed);
        tubes[tubeCount].idxInUpdateArray = tubeCount;
        tubeCount++;
        return ret;
    }
    
    public int addGenerator(ushort _itemId)
    {
        // when we create a new generator, it should claim a space in the endStates array as indicated by the index 'generators[i].idxInOutputStateArray'.
        int ret = generators.Count;
        generators.Add(new GeneratorData());
        GeneratorData gen = generators[ret];
        gen.init(_itemId, 2f, 3, ret, registerEndState());
        gen.start();
        generators[ret] = gen;
        return ret;
    }

    public int addConverter() {
        int ret = converters.Count;
        converters.Add(new ConverterData());
        ConverterData conv = converters[ret];
        conv.init(1f, 1, registerEndState());
        conv.setItemRequirements(new ushort[] { 1, 2 }, new byte[] { 2, 3 }, 1, 1);
        converters[ret] = conv;
        return ret;
    }

    public void addConsumer() {

    }
    public int registerEndState() {
        int ret = outputStateCount;
        outputStateCount++;
        return ret;
    }
    
    private void OnDestroy()
    {
        tempOps.Dispose();
        tubeUpdateData.Dispose();
        generic[0].Dispose();
        generic[1].Dispose();
    }
   
    void Update() {
        sampler.Begin();
        if (addnew)
        {
            addnew = false;
            //for (int i = 0; i < arrayLength; ++i)
            //{
            //    tubes[i].push();
            //}
            //tubes[0].push();
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
        updateTubesJob.Run(tubeCount);
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
#if !DEBUG_JOB
        updateTubesJH.Complete();
#endif

        // generator output
        for (int i = 0; i < generic[0].tempGenericUpdateOps.Length; ++i) {
            if (generic[0].tempGenericUpdateOps[i] != 0) {
                GeneratorData gen = generators[i];
                CanTakeState cts = endStates[gen.idxInOutputStateArray];
                // check if this generator has space at its end?
                if (tubes[cts.idx].hasSpace(gen.itemId)) {
                    tubes[cts.idx].push(gen.itemId);
                    // consume a unit of resource.
                    gen.onExpended();
                    generators[i] = gen;
                }
            }
        }

        // tubes output
        for (int i = 0; i < arrayLength; ++i)
        {
            int op = updateTubesJob.outputOps[i];
            if (op != 0)
            {
                CanTakeState cts = endStates[tubes[i].idxInOutputStateArray];
                TubeData srcTube = tubes[i];
                if (cts.type == 0) // tube
                {
                    if (tubes[cts.idx].hasSpace(srcTube.itemId))
                    {
                        srcTube.pop();
                        tubes[cts.idx].push(srcTube.itemId);
                        // consume a unit of resource.
                    }
                    else
                    {
                        srcTube.saturate();
                    }
                    tubes[i] = srcTube;
                }
                else if(cts.type == 1)
                {
                    ConverterData conv = converters[cts.idx];
                    if (conv.hasSpace(srcTube.itemId)) {
                        srcTube.pop();
                        conv.push(srcTube.itemId);
                        converters[cts.idx] = conv;

                    }
                    else {
                        srcTube.saturate();
                    }
                    tubes[i] = srcTube;
                }
            }
        }

        // converter output
        for (int i = 0; i < generic[1].tempGenericUpdateOps.Length; ++i) {
            if (generic[1].tempGenericUpdateOps[i] != 0) {
                CanTakeState cts = endStates[converters[i].idxInOutputStateArray];
                ConverterData conv = converters[i];
                // check if this generator has space at its end?
                if (cts.type == 0) { // tube
                    if (tubes[cts.idx].hasSpace(conv.targetId)) {
                        Debug.Log("converter out!");
                        tubes[cts.idx].push(conv.targetId);
                        
                        conv.clearCurrent();
                        converters[i] = conv;
                    }
                    else
                    {

                    }
                }
                else if(cts.type == 1) { // converter

                }
            }
        }


        sampler.End();
        // debug
        for (int j = 0; j < tubeCount; ++j)
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
