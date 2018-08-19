using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public class Storage : MonoBehaviour, ISimView {
        public StorageData target;

        public int count;
        private void Update() {
            count = target.stacks[1];
        }
        public ISimData getTarget() {
            return target;
        }
    }
}