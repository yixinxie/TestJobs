using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
public interface IUpdate {
    void PerFrameUpdate(float dt);
}
public class SystemUpdate : MonoBehaviour {
    public static SystemUpdate self;
    List<IUpdate> systems;

    void Awake() {
        self = this;
        systems = new List<IUpdate>(32);
        systems.Add(new Simulation_OOP.FloatUpdate());
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

    void Update() {
        float dt = Time.deltaTime;
        for(int i = 0; i < systems.Count; ++i) {
            systems[i].PerFrameUpdate(dt);
        }
    }
}
