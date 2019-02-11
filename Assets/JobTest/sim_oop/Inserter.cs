using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public class Inserter : MonoBehaviour, ISimView {
        public InserterData target;
        public string head;
        public string tail;
        // debug
        public float phaseTime;
        public int phase;
        private void Update() {
            phaseTime = target.getTimeLeft();
            phase = target.phase;
        }
        public ISimData getTarget() {
            return target;
        }
    }
}