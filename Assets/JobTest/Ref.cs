//#define DEBUG_JOB
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using Unity.Jobs;
using Unity.Collections;

public enum SyncModes
{
    All,
    OwnerOnly,
    RemoteOnly,
}
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public class Replicated : Attribute
{
    int syncMode;

    public virtual SyncModes Mode
    {
        get { return (SyncModes)syncMode; }
        set { syncMode = (int)value; }
    }
}
public class Ref : MonoBehaviour {
    [Replicated(Mode = SyncModes.OwnerOnly)]
    public int testInt;
    private void Awake()
    {
        self = this;
    }
    // Use this for initialization
    void Start () {
        const BindingFlags flags = /*BindingFlags.NonPublic | */BindingFlags.Public |
             BindingFlags.Instance | BindingFlags.Static;
        UnityEngine.Object obj = this;
        FieldInfo[] fields = obj.GetType().GetFields(flags);
        foreach (FieldInfo fieldInfo in fields)
        {
            //Attribute
            Attribute[] attributes = fieldInfo.GetCustomAttributes(typeof(Attribute), true) as Attribute[];
            if (attributes == null || attributes.Length == 0) continue;
            Replicated repAttr = attributes[0] as Replicated;
            Debug.Log(repAttr.Mode.ToString());
        }

        tubes = new TubeData[objCount];
        tubeUpdateData = new NativeArray<TubeUpdateData>(objCount, Allocator.Persistent);
        endStates = new NativeArray<byte>(objCount, Allocator.Persistent);
        tempOps = new NativeArray<byte>(objCount, Allocator.Persistent);
        for (int i = 0; i < objCount; ++i)
        {
            tubes[i].init(10.0f, 2.0f);
            tubes[i].idxInUpdateArray = i;
            tubes[i].push();
            endStates[i] = 0;
        }
    }
    private void OnDestroy()
    {
        tempOps.Dispose();
        endStates.Dispose();
        tubeUpdateData.Dispose();
    }
    public static Ref self;
    public int objCount = 10000;
    TubeData[] tubes;
    public NativeArray<TubeUpdateData> tubeUpdateData;
    TubeUpdateJob updateJob;
    JobHandle updateJH;
    public NativeArray<byte> endStates;
    public NativeArray<byte> tempOps;
    // Update is called once per frame
    float elapsedTime;
    bool pushed;
    public bool addnew;
    public int currentIndex0;
    public int count0;
    
    void Update() {

        if (addnew)
        {
            addnew = false;
            for (int i = 0; i < objCount; ++i)
            {
                tubes[i].push();
            }
        }
        elapsedTime += Time.deltaTime;
        if(elapsedTime > 2.0f && pushed == false)
        {
            pushed = true;
            for (int i = 0; i < objCount; ++i)
            {
                tubes[i].push();
            }
        }
        updateJob = new TubeUpdateJob()
        {
            deltaTime = Time.deltaTime,
            dataArray = tubeUpdateData,
            outputOps = tempOps,
        };
#if DEBUG_JOB
        updateJob.Run(objCount);
#else
        updateJH = updateJob.Schedule(objCount, 64);
#endif
    }

    private void LateUpdate()
    {
#if DEBUG_JOB
#else
        updateJH.Complete();
#endif


        for (int i = 0; i < objCount; ++i)
        {
            int op = updateJob.outputOps[i];
            if (op != 0)
            {
                byte state = endStates[i];
                //if (endStates[index] == 0)
                //{
                //    // the end point is empty, let the tube remove this element and assign a new one.
                //    op = 1;

                //}
                //else if (endStates[index] == 1)
                //{
                //    // this element is blocked, let the tube update the element before this.
                //    op = 2;
                //}

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
        for (int j = 0; j < objCount; ++j)
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
            }
        }
        dbgArray = tubes[0].positions;
    }
    public float[] dbgArray;
    public bool toggleEndState;
    public int endState0;
    
    struct TubeUpdateJob : IJobParallelFor
    {
        [ReadOnly]
        public float deltaTime;
        //[ReadOnly]
        //public NativeArray<byte> endStates;
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
                if (thisData.current >= thisData.boundary)
                {
                    op = 1;
                }
            }
            outputOps[index] = op;
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