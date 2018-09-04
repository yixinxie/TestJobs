using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public class Storage : MonoBehaviour, ISimView {
        public StorageData target;
        // debug
        public short[] counts;
        private void Update() {
            counts = target.stacks;
        }
        public ISimData getTarget() {
            return target;
        }
    }
}