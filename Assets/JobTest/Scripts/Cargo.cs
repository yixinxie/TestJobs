using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class Cargo : MonoBehaviour {
    

    // Start is called before the first frame update
    void Start()
    {
        AcceleratedMovement.self.AddMovementInfo(this.transform, Vector3.zero, 10f, Vector3.right * 10f, linearDone);
        
    }
    public void linearDone() {
        LinearMovement.self.AddMovementInfo(this.transform, Vector3.one * 10f, 1f, null);
    }
}
public class LinearMovement : IJobUpdate {
    TransformAccessArray taa;
    NativeArray<Vector3> targets;
    NativeArray<float> speeds;
    NativeArray<byte> terminated;
    List<Action> callbacks;
    int count;
    LinearMovementJob job;
    JobHandle jh;
    List<int> to_remove;
    public static LinearMovement self;
    public LinearMovement(int defaultCount = 64) {
        self = this;
        taa = new TransformAccessArray(defaultCount);
        targets = new NativeArray<Vector3>(defaultCount, Allocator.Persistent);
        speeds = new NativeArray<float>(defaultCount, Allocator.Persistent);
        terminated = new NativeArray<byte>(defaultCount, Allocator.Persistent);
        callbacks = new List<Action>(defaultCount);
        to_remove = new List<int>();
    }
    public void AddMovementInfo(Transform trans, Vector3 targetPos, float speed, Action doneCallback) {
        taa.Add(trans);
        targets[count] = targetPos;
        speeds[count] = speed;
        terminated[count] = 0;
        callbacks.Add(doneCallback);
        count++;
    }
    public void Dispose() {
        targets.Dispose();
        taa.Dispose();
        speeds.Dispose();
        terminated.Dispose();
    }
    public void FirstPass(float dt) {
        job = new LinearMovementJob() {
            dt = dt,
            targets = targets,
            speeds = speeds,
            terminated = terminated,
        };
        jh = job.Schedule(taa);
    }
    
    public void SecondPass(float dt) {
        jh.Complete();
        to_remove.Clear();
        for(int i = 0;i < count; ++i) {
            if (terminated[i] == 1) {
                if (callbacks[i] != null)
                    callbacks[i].Invoke();
                to_remove.Add(i);
                Debug.Log("linear move remove " + i);
                terminated[i] = 0;
            }
        }
        for(int i = to_remove.Count - 1; i >= 0; --i) {
            int removeIdx = to_remove[i];
            taa.RemoveAtSwapBack(removeIdx);
            targets[removeIdx] = targets[count - 1];
            speeds[removeIdx] = speeds[count - 1];
            callbacks[removeIdx] = callbacks[count - 1];
            count--;
        }
    }
    struct LinearMovementJob : IJobParallelForTransform {
        public float dt;
        public NativeArray<float> speeds;
        public NativeArray<Vector3> targets;
        [WriteOnly]
        public NativeArray<byte> terminated;
        public void Execute(int index, TransformAccess transform) {
            Vector3 thisDir = targets[index] - transform.localPosition;
            float mag = thisDir.magnitude;
            if (mag < speeds[index] * dt) {
                transform.localPosition = targets[index];
                terminated[index] = 1;
            }
            else {
                thisDir /= mag;
                Vector3 inc = thisDir * dt;
                transform.localPosition += inc;
                terminated[index] = 0;
            }
            
        }
    }
}
public class AcceleratedMovement : IJobUpdate {
    TransformAccessArray taa;
    NativeArray<Vector3> velocities;
    NativeArray<float> accelerations;
    NativeArray<Vector3> target_velocities;
    List<Action> callbacks;
    NativeArray<byte> terminated;
    int count;
    AcceleratedMovementJob job;
    JobHandle jh;
    
    List<int> to_remove;
    public static AcceleratedMovement self;
    public AcceleratedMovement(int defaultCount = 64) {
        self = this;
        taa = new TransformAccessArray(defaultCount);
        velocities = new NativeArray<Vector3>(defaultCount, Allocator.Persistent);
        accelerations = new NativeArray<float>(defaultCount, Allocator.Persistent);
        target_velocities = new NativeArray<Vector3>(defaultCount, Allocator.Persistent);
        terminated = new NativeArray<byte>(defaultCount, Allocator.Persistent);
        callbacks = new List<Action>();
        to_remove = new List<int>();
    }
    public void AddMovementInfo(Transform trans, Vector3 velocity, float _acceleration, Vector3 targetVelocity, Action done) {
        taa.Add(trans);
        velocities[count] = velocity;
        accelerations[count] = _acceleration;
        target_velocities[count] = targetVelocity;
        terminated[count] = 0;
        callbacks.Add(done);
        count++;
    }
    public void Dispose() {
        velocities.Dispose();
        taa.Dispose();
        accelerations.Dispose();
        target_velocities.Dispose();
        terminated.Dispose();
    }
    public void FirstPass(float dt) {
        job = new AcceleratedMovementJob() {
            dt = dt,
            velocities = velocities,
            target_velocities = target_velocities,
            accelerations = accelerations,
            terminated = terminated,
        };
        jh = job.Schedule(taa);
    }
    
    public void SecondPass(float dt) {
        jh.Complete();
        to_remove.Clear();
        for (int i = 0; i < count; ++i) {
            if (terminated[i] == 1) {
                if(callbacks[i] != null)
                    callbacks[i].Invoke();
                to_remove.Add(i);
                Debug.Log("acc move remove " + i);
                terminated[i] = 0;
            }
        }
        for (int i = to_remove.Count - 1; i >= 0; --i) {
            int removeIdx = to_remove[i];
            taa.RemoveAtSwapBack(removeIdx);
            velocities[removeIdx] = velocities[count - 1];
            target_velocities[removeIdx] = target_velocities[count - 1];
            accelerations[removeIdx] = accelerations[count - 1];
            callbacks[removeIdx] = callbacks[count - 1];
            count--;
        }
    }
    struct AcceleratedMovementJob : IJobParallelForTransform {
        [ReadOnly]
        public float dt;
        [ReadOnly]
        public NativeArray<float> accelerations;
        [ReadOnly]
        public NativeArray<Vector3> target_velocities;

        [WriteOnly]
        public NativeArray<byte> terminated;

        public NativeArray<Vector3> velocities;

        public void Execute(int index, TransformAccess transform) {
            Vector3 diff = target_velocities[index] - velocities[index];
            float mag = diff.magnitude;
            if (mag < accelerations[index] * dt) {
                velocities[index] = target_velocities[index];
                terminated[index] = 1;
            }
            else {
                diff /= mag;
                velocities[index] += diff * accelerations[index] * dt;
                terminated[index] = 0;
            }
            transform.localPosition += velocities[index] * dt;
        }
    }
}