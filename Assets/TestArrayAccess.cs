using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class TestArrayAccess : MonoBehaviour {
    public int count;
    //float[] array;
    public bool useJob;
    public float outp;
    public Text textref;
    NativeArray<float> floatValues;
    // Use this for initialization
    private void Awake() {
        //array = new float[count];
        floatValues = new NativeArray<float>(count, Allocator.Persistent);
        for (int i = 0; i < count; i++) {
            floatValues[i] = 0f;
            //array[i] = 0f;
        }
    }
    private void OnDestroy() {
        jobHandle.Complete();
        floatValues.Dispose();
    }
    int c = 0;
    JobHandle jobHandle;
	// Update is called once per frame
	void Update () {
        float dt = Time.deltaTime;
        float sum = 0f;
        //float begin = Time.realtimeSinceStartup;
        //if (reversed) {
        //    for(int i = count - 1; i >= 0; --i) {
        //        array[i] += dt;
        //        sum += array[i];
        //    }
        //    //outp = sum;
        //}
        //else {
        //    for (int i = 0; i < count; ++i) {
        //        array[i] += dt;
        //        sum += array[i];
        //    }
        //    //outp = sum;
        //}
        if (useJob) {
//            if (jobHandle.IsCompleted) 
            {
                jobHandle.Complete();
                // Initialize the job data
                TestArrayWorker job = new TestArrayWorker() {
                    deltaTime = dt,
                    floats = floatValues,
                };
                // Schedule the job, returns the JobHandle which can be waited upon later on
                jobHandle = job.Schedule();
            }
            
            //job.Run<TestArrayWorker>();
        }
        else {
            int c = floatValues.Length;
            for (int i = 0; i < count; ++i) {
                floatValues[i] += dt;
                //sum += array[i];
            }
        }
        //float end = Time.realtimeSinceStartup;
        //c++;
        //if ((c % 60) == 0) {
        //    textref.text = "time: " + (end - begin);
        //}

    }
    public struct TestArrayWorker : IJob {
        //[ReadOnly]
        public NativeArray<float> floats;
        [ReadOnly]
        public float deltaTime;
        public float sum;

        // The code actually running on the job
        public void Execute() {
            int c = floats.Length;
            float _sum = 0f;
            float dt = deltaTime;
            // Move the positions based on delta time and velocity
            for (int i = 0; i < c; ++i) {
                floats[i] = floats[i] + dt;
                //_sum += floats[i];
            }
            sum = _sum;
        }
    }
}
