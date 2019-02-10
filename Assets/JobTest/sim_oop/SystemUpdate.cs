using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
public interface IUpdate {
    void PerFrameUpdate(float dt);
}

public interface IJobUpdate {
    void FirstPass(float dt);
    void SecondPass(float dt);
    void Dispose();
}
public class SystemUpdate : MonoBehaviour {
    public static SystemUpdate self;
    List<IUpdate> systems;
    List<IJobUpdate> jobs;

    void Awake() {
        self = this;
        systems = new List<IUpdate>(32);
        systems.Add(new Simulation_OOP.FloatUpdate(4096 * 8));

        jobs = new List<IJobUpdate>(32);
        jobs.Add(new LinearMovement());
        jobs.Add(new AcceleratedMovement());
    }
    private void Start() {
        
    }

    // for monobehavior systems, use this.
    public void RegisterPerFrameUpdate(IUpdate system) {
        systems.Add(system);
    }

    // normally not useful.
    public void UnregisterPerFrameUpdate(IUpdate system) {
        systems.Remove(system);
    }
    float dt;
    void Update() {
        dt = Time.deltaTime;
        for(int i = 0; i < systems.Count; ++i) {
            systems[i].PerFrameUpdate(dt);
        }
        for (int i = 0; i < jobs.Count; ++i) {
            jobs[i].FirstPass(dt);
        }
    }
    private void LateUpdate() {
        for (int i = 0; i < jobs.Count; ++i) {
            jobs[i].SecondPass(dt);
        }
    }
    private void OnDestroy() {
        for (int i = 0; i < jobs.Count; ++i) {
            jobs[i].Dispose();
        }
    }
}
