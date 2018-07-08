#define DEBUG_JOB
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Profiling;

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
    public CanTakeState[] headStates;
    public int outputStateCount;

    /* generator related */
    GeneratorData[] generators;
    /* converter related*/
    ConverterData[] converters;
    ConsumerData[] consumers;
    int generatorCount;
    int converterCount;
    int consumerCount;
    /* debug */
    CustomSampler sampler;
    public float elapsedTime;
    bool pushed;
    public bool addnew;
    public int currentIndex0;
    public int count0;
    public float[] dbgArray;

    public Object objPrefab;
    List<GameObject> itemObjs;
    private void Awake()
    {
        self = this;

        tubes = new TubeData[arrayLength];
        tubeUpdateData = new NativeArray<TubeUpdateData>(arrayLength, Allocator.Persistent);
        tempOps = new NativeArray<byte>(arrayLength, Allocator.Persistent);

        headStates = new CanTakeState[arrayLength];

        generic = new GenericJob[2];
#if DEBUG_JOB
        generic[0] = new GenericJob(true);
        generic[1] = new GenericJob(true);
#else
        generic[0] = new GenericJob(false);
        generic[1] = new GenericJob(false);
#endif

        generators = new GeneratorData[arrayLength];
        converters = new ConverterData[arrayLength];
        consumers = new ConsumerData[arrayLength];
        itemObjs = new List<GameObject>(16);
    }
    // Use this for initialization
    void Start () {
        sampler = UnityEngine.Profiling.CustomSampler.Create("Job System");
        outputStateCount = 0;
        treeTest(1);
        //simpleTest();
        //tubeTest();
    }
    void tubeTest() {
        int genId = addGenerator(50);
        int tubeId = addTube(5f, 2f);
        linkTubeToGenerator(genId, tubeId);
    }
    void simpleTest() {
        addGenerator(1);
        for (int i = 0; i < arrayLength; ++i) {
            addTube(5.0f, 2.0f);
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
        int[] generatorIds = new int[generatorCount];
        for (int i = 0; i < generatorCount; ++i) {

            generatorIds[i] = addGenerator((ushort)(i + 1)); 
        }

        // make tubes the same amount as the generators
        List<int> tubeIds = new List<int>();
        for (int i = 0; i < generatorCount; ++i) {
            int tubeId = addTube(5f, 2f);
            tubeIds.Add(tubeId);

            linkTubeToGenerator(generatorIds[i], tubeId);
        }

        // make one converter for every two tubes and connect them.
        List<int> converterIds = new List<int>();
        for (int i = 0; i < tubeIds.Count / 2; ++i) {

            int convId = addConverter();
            converterIds.Add(convId);
            //linkConverterToTube(tubeIds[i * 2], convId);
            int[] tmp = new int[2] { tubeIds[i * 2], tubeIds[i * 2 + 1] };
            linkConverterToTubes(tmp, convId);
        }

        //make one tube for each converter
        List<int> tubeIds2 = new List<int>();
        for (int i = 0; i < converterIds.Count; ++i) {
            int tubeId = addTube(2f, 2f);
            tubeIds2.Add(tubeId);
            linkTubeToConverter(converterIds[i], tubeId);
        }
        List<int> consumerIds = new List<int>();
        // make one storage for each tube
        for (int i = 0; i < tubeIds2.Count; ++i) {
            int consumerId = addConsumer();
            consumerIds.Add(consumerId);
            linkConsumerToTube(tubeIds2[i], consumerId);
        }
    }
    public const byte CTS_None = 255;
    public const byte CTS_Tube = 0;
    public const byte CTS_Converter = 1;
    public const byte CTS_Consumer = 2;
    public const byte CTS_Generator = 4;

    // link converter to tube
    void linkConverterToTube(int tubeIdx, int converterIdx) // tube ==> converter
    {
        TubeData tube = tubes[tubeIdx];
        CanTakeState cts = headStates[tube.tailArrayIdx];

        cts.tailType = CTS_Converter;
        cts.tailIdx = converterIdx;

        cts.addNewHead(CTS_Tube, tubeIdx);
        headStates[tube.tailArrayIdx] = cts;

        converters[converterIdx].headArrayIdx = tube.tailArrayIdx;
    }
    void linkConverterToTubes(int[] tubeIdx, int converterIdx) // tubes ==> converter
    {
        ConverterData conv = converters[converterIdx];
        
        //TubeData tube = tubes[tubeIdx];
        CanTakeState cts = headStates[conv.headArrayIdx];
        cts.init();
        cts.tailType = CTS_Converter;
        cts.tailIdx = converterIdx;

        for (int i = 0; i < tubeIdx.Length; ++i) {
            cts.addNewHead(CTS_Tube, tubeIdx[i]);
            TubeData tube = tubes[tubeIdx[i]];
            tube.tailArrayIdx = conv.headArrayIdx;
            tubes[tubeIdx[i]] = tube;
        }
        headStates[conv.headArrayIdx] = cts;

        //converters[converterIdx].idxHeadInEndStateArray = tube.idxInEndStateArray;
    }
    void linkTubeToConverter(int converterIdx, int tubeIdx) // converter ==> tube
    {
        CanTakeState cts = headStates[tubes[tubeIdx].headArrayIdx];
        cts.init();
        cts.tailType = CTS_Tube;
        cts.tailIdx = tubeIdx;
        cts.addNewHead(CTS_Converter, converterIdx);
        headStates[tubes[tubeIdx].headArrayIdx] = cts;

        ConverterData conv = converters[converterIdx];
        conv.idxInEndStateArray = tubes[tubeIdx].headArrayIdx;
        converters[converterIdx] = conv;
    }
    

    void linkTubeToGenerator(int generatorIdx, int tubeIdx) // generator ==> tube
    {
        CanTakeState cts = headStates[tubes[tubeIdx].headArrayIdx];
        cts.init();
        cts.tailType = CTS_Tube;
        cts.tailIdx = tubeIdx;
        cts.addNewHead(CTS_Generator, generatorIdx);
        headStates[tubes[tubeIdx].headArrayIdx] = cts;

        GeneratorData generator = generators[generatorIdx];
        generator.tailArrayIdx = tubes[tubeIdx].headArrayIdx;
        generators[generatorIdx] = generator;

        //tubes[tubeIdx].headArrayIdx = 
    }

    void linkTubeToTube(int tubeIdxPre, int tube1IdxSucc) // tube ==> tube
    {
        CanTakeState cts = headStates[tubes[tube1IdxSucc].headArrayIdx];
        cts.init();
        cts.tailType = CTS_Tube;
        cts.tailIdx = tube1IdxSucc;

        cts.addNewHead(CTS_Tube, tubeIdxPre);
        headStates[tubes[tube1IdxSucc].headArrayIdx] = cts;

        tubes[tubeIdxPre].tailArrayIdx = tubes[tube1IdxSucc].headArrayIdx;
    }
    
    // consumer to tube
    void linkConsumerToTube(int tubeIdx, int consumerIdx) // tube ==> consumer
    {
        //TubeData tube = tubes[tubeIdx];
        //CanTakeState cts = endStates[tube.idxInEndStateArray];
        //cts.init();
        //cts.tailType = CTS_Consumer;
        //cts.tailIdx = consumerIdx;

        //cts.addNewHead(CTS_Tube, tubeIdx);
        //endStates[tube.idxInEndStateArray] = cts;


        CanTakeState cts = headStates[consumers[consumerIdx].headArrayIdx];
        cts.init();
        cts.tailType = CTS_Consumer;
        cts.tailIdx = consumerIdx;
        cts.addNewHead(CTS_Tube, tubeIdx);
        headStates[tubes[tubeIdx].headArrayIdx] = cts;

        TubeData tube = tubes[tubeIdx];
        tube.tailArrayIdx = consumers[consumerIdx].headArrayIdx;
        tubes[tubeIdx] = tube;

    }

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

        tubes[tubeCount].headArrayIdx = registerHeadState();
        tubeCount++;
        return ret;
    }
    
    public int addGenerator(ushort _itemId)
    {
        // when we create a new generator, it should claim a space in the endStates array as indicated by the index 'generators[i].idxInOutputStateArray'.
        int ret = generatorCount;
        generatorCount++;
        //generators.Add(new GeneratorData());
        GeneratorData gen = generators[ret];
        gen.init(_itemId, 2f, 9999);
        gen.start();
        generators[ret] = gen;
        return ret;
    }

    public int addConverter() {
        int ret = converterCount;
        converterCount++;
        //converters.Add(new ConverterData());
        ConverterData conv = converters[ret];
        conv.init(99f, registerHeadState());
        conv.setItemRequirements(new ushort[] { 1, 2 }, new byte[] { 2, 3 }, 1, 1);
        converters[ret] = conv;
        return ret;
    }

    public int addConsumer() {
        //int ret = ConsumerData\
        int ret = consumerCount;
        //consumers.Add(new ConsumerData());
        consumerCount++;
        ConsumerData newStruct = new ConsumerData();
        newStruct.init(registerHeadState());
        
        consumers[ret] = newStruct;
        return ret;
    }
    public int registerHeadState() {
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
        //float deltaTime = Time.deltaTime;
        if(elapsedTime > 9.733f) {
            int sdf = 0;
        }
        float deltaTime = 0.016667f;
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
            deltaTime = deltaTime,
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
    public bool endState_Open;
    bool prevEndState0;
    void debugTube() {
        // generator
        for (int i = 0; i < generic[0].tempGenericUpdateOps.Length; ++i) {
            if (generic[0].tempGenericUpdateOps[i] != 0) {
                GeneratorData gen = generators[i];
                CanTakeState cts = headStates[gen.tailArrayIdx];
                // check if this generator has space at its end?
                if (tubes[cts.tailIdx].hasSpace(gen.itemId)) {
                    tubes[cts.tailIdx].push(gen.itemId);
                    // consume a unit of resource.
                    gen.pop();

                    generators[i] = gen;
                }
                else {
                    gen.block();
                }
            }
        }
        for (int i = 0; i < arrayLength; ++i) {
            int op = updateTubesJob.outputOps[i];
            if (op != 0) {
                TubeData srcTube = tubes[i];
                if (endState_Open) {
                    srcTube.pop();
                    CanTakeState cts = headStates[srcTube.headArrayIdx];
                    cts.invokeUnblock();
                }
                else {
                    srcTube.block();
                }
                        
                //CanTakeState cts = endStates[tubes[i].idxInEndStateArray];

                //if (cts.type == 0) // tube
                //{
                //    if (tubes[cts.idx].hasSpace(srcTube.itemId)) {
                //        srcTube.pop();
                //        tubes[cts.idx].push(srcTube.itemId);
                //        // consume a unit of resource.
                //    }
                //    else {
                //        srcTube.saturate();
                //    }
                //}
                //else if (cts.type == 1) // converter
                //{
                //    ConverterData conv = converters[cts.idx];
                //    if (conv.hasSpace(srcTube.itemId)) {
                //        srcTube.pop();
                //        conv.push(srcTube.itemId);
                //        converters[cts.idx] = conv;

                //    }
                //    else {
                //        srcTube.saturate();
                //    }
                //}
                //else if (cts.type == 2) { // consumer
                //    ConsumerData consData = consumers[cts.idx];
                //    consData.attemptToTake(srcTube.itemId);
                //    srcTube.pop();
                //    consumers[cts.idx] = consData;
                //}
                tubes[i] = srcTube;
            }
        }
        if (prevEndState0 != endState_Open) {
            if (endState_Open) {
                tubes[0].unblock();
                CanTakeState cts = headStates[tubes[0].headArrayIdx];
                cts.invokeUnblock();
            }
            prevEndState0 = endState_Open;
        }

    }

    List<Vector3> itemPositions = new List<Vector3>(16);
    private void LateUpdate() {
        sampler.Begin();
        generic[0].lateUpdate();
        generic[1].lateUpdate();
#if !DEBUG_JOB
        updateTubesJH.Complete();
#endif
        //debugTube();

        postJobUpdate();
        // debug
        float itemHeight = 0.13f;
        itemPositions.Clear();
        for (int j = 0; j < tubeCount; ++j) {
            TubeData d = tubes[j];
            float offset = d.getOffset();
            for (int i = 0; i < d.count; ++i) {
                Vector3 pos = Vector3.zero;
                if (i > d.currentIndex) {
                    pos = new Vector3(d.positions[i], itemHeight, j);
                }
                else {
                    pos = new Vector3(d.positions[i] + offset, itemHeight, j);
                }
                itemPositions.Add(pos);
                //Debug.DrawLine(pos, pos + Vector3.up, Color.green);
            }
        }
        
        for(int i = 0; i < itemPositions.Count; ++i) {
            if(i >= itemObjs.Count) {
                GameObject newItemObj = GameObject.Instantiate(objPrefab, this.transform) as GameObject;
                itemObjs.Add(newItemObj);
            }
            itemObjs[i].transform.localPosition = itemPositions[i];
        }
        for(int i = itemPositions.Count; i < itemObjs.Count; ++i) {
            itemObjs[i].SetActive(false);
        }
        currentIndex0 = tubes[0].currentIndex;
        count0 = tubes[0].count;

        dbgArray = tubes[0].positions;
    }
    Transform getItem() {
        for(int i = 0; i< itemObjs.Count; ++i) {
            if(itemObjs[i].activeSelf == false) {
                return itemObjs[i].transform;
            }
        }
        GameObject newItemObj = GameObject.Instantiate(objPrefab, this.transform) as GameObject;
        itemObjs.Add(newItemObj);
        return newItemObj.transform;
    }
    void postJobUpdate() {
        // generator output
        for (int i = 0; i < generic[0].tempGenericUpdateOps.Length; ++i) {
            if (generic[0].tempGenericUpdateOps[i] != 0) {
                GeneratorData gen = generators[i];
                CanTakeState cts = headStates[gen.tailArrayIdx];
                // check if this generator has space at its end?
                if (tubes[cts.tailIdx].hasSpace(gen.itemId)) {
                    tubes[cts.tailIdx].push(gen.itemId);
                    // consume a unit of resource.
                    gen.pop();
                    generators[i] = gen;
                }
            }
        }

        // tubes output
        for (int i = 0; i < arrayLength; ++i)
        {
            if (updateTubesJob.outputOps[i] != 0)
            {
                CanTakeState cts = headStates[tubes[i].tailArrayIdx];
                TubeData srcTube = tubes[i];
                if (cts.tailType == 0) // tube
                {
                    if (tubes[cts.tailIdx].hasSpace(srcTube.itemId))
                    {
                        srcTube.pop();
                        tubes[cts.tailIdx].push(srcTube.itemId);

                        cts = headStates[srcTube.headArrayIdx];
                        cts.invokeUnblock();
                    }
                    else
                    {
                        srcTube.block();
                    }
                }
                else if(cts.tailType == 1) // converter
                {
                    ConverterData conv = converters[cts.tailIdx];
                    if (conv.hasSpace(srcTube.itemId)) {
                        srcTube.pop();
                        converters[cts.tailIdx].push(srcTube.itemId);

                        cts = headStates[srcTube.headArrayIdx];
                        cts.invokeUnblock();

                    }
                    else {
                        srcTube.block();
                    }
                }
                else if(cts.tailType == 2) { // consumer
                    ConsumerData consData = consumers[cts.tailIdx];
                    consData.attemptToTake(srcTube.itemId);
                    srcTube.pop();
                    consumers[cts.tailIdx] = consData;

                    cts = headStates[srcTube.headArrayIdx];
                    cts.invokeUnblock();
                }
                tubes[i] = srcTube;
            }
        }

        // converter output
        for (int i = 0; i < generic[1].tempGenericUpdateOps.Length; ++i) {
            if (generic[1].tempGenericUpdateOps[i] != 0) {
                CanTakeState cts = headStates[converters[i].idxInEndStateArray];
                ConverterData conv = converters[i];
                // check if this converter has space at its end?
                if (cts.tailType == 0) { // the end entity of this converter is a tube
                    if (tubes[cts.tailIdx].hasSpace(conv.targetId)) {
                        Debug.Log("converter out!");
                        tubes[cts.tailIdx].push(conv.targetId);
                        conv.clearCurrent();
                        converters[i] = conv;

                        headStates[conv.headArrayIdx].invokeUnblock();
                    }
                    else
                    {
                        //head_cts
                    }
                }
                else if(cts.tailType == 1) { // converter

                }
            }
        }


        sampler.End();
        
    }

    public struct CanTakeState {
        public int tailIdx; // index in the respective array.
                            //public byte cachedState;

        public int headIdx_0;
        public int headIdx_1;
        public int headIdx_2;
        public int headIdx_3;

        public byte tailType; // 0 : tube, 1 : converter, 2 : consumer
        public byte headType_0;
        public byte headType_1;
        public byte headType_2;
        public byte headType_3;
        public void init() {
            headType_0 = TubeSimulate.CTS_None;
            headType_1 = TubeSimulate.CTS_None;
            headType_2 = TubeSimulate.CTS_None;
            headType_3 = TubeSimulate.CTS_None;
            tailType = TubeSimulate.CTS_None;
            tailIdx = -1;
            headIdx_0 = -1;
            headIdx_1 = -1;
            headIdx_2 = -1;
            headIdx_3 = -1;
        }
        private void invokeUnblockSingle(byte type, int idx) {
            if (type == TubeSimulate.CTS_Tube) {
                TubeSimulate.self.tubes[idx].unblock();
            }
            else if (type == TubeSimulate.CTS_Converter) {
                TubeSimulate.self.converters[idx].unblock();

            }
            if (type == TubeSimulate.CTS_Consumer) {
                TubeSimulate.self.consumers[idx].unblock();

            }
            if (type == TubeSimulate.CTS_Generator) {
                TubeSimulate.self.generators[idx].unblock();

            }
        }
        public void invokeUnblock() {
            if (headType_0 != TubeSimulate.CTS_None) {
                invokeUnblockSingle(headType_0, headIdx_0);
            }
            else if (headType_1 == TubeSimulate.CTS_None) {
                invokeUnblockSingle(headType_1, headIdx_1);
            }
            else if (headType_2 == TubeSimulate.CTS_None) {
                invokeUnblockSingle(headType_2, headIdx_2);
            }
            else if (headType_3 == TubeSimulate.CTS_None) {
                invokeUnblockSingle(headType_3, headIdx_3);
            }
        }
        public void addNewHead(byte type, int idx) {
            if (headType_0 == TubeSimulate.CTS_None) {
                headType_0 = type;
                headIdx_0 = idx;
            }
            else if (headType_1 == TubeSimulate.CTS_None) {
                headType_1 = type;
                headIdx_1 = idx;
            }
            else if (headType_2 == TubeSimulate.CTS_None) {
                headType_2 = type;
                headIdx_2 = idx;
            }
            else if (headType_3 == TubeSimulate.CTS_None) {
                headType_3 = type;
                headIdx_3 = idx;
            }
        }


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
