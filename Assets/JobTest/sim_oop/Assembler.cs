using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public class Assembler : MonoBehaviour, ISimView {
        public AssemblerData target;
        public int totalProduced;
        public float left;
        public void Update() {
            left = target.timeLeft;
            totalProduced = target.totalProduced;
        }
        public ISimData getTarget() {
            return target;
        }
    }
    public interface IFloatUpdate {
        void NotifyIndexChange(int new_idx);
        void Zero(float left);

    }
    public class FloatUpdate {
        float[] values;
        IFloatUpdate[] subs;
        int count;
        public FloatUpdate(int defaultLength = 16) {
            values = new float[defaultLength];
            subs = new IFloatUpdate[defaultLength];
        }
        public void Add(IFloatUpdate sub, float val) {
            values[count] = val;
            subs[count] = sub;
            count++;
        }
        public void RemoveAt(int idx) {
            count--;
            values[idx] = values[count];
            subs[idx] = subs[count];
            subs[idx].NotifyIndexChange(idx);
        }

        public void update(float dt) {
            float _count = count;
            for(int i = 0; i < _count; ++i) {
                values[i] -= dt;
                if(values[i] <= 0.0f) {
                    subs[i].Zero(values[i]);
                }
            }
        }
        
    }

}