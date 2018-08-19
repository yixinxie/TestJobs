using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public class Inserter : MonoBehaviour, ISimView {
        public InserterData target;
        public string head;
        public string tail;
        public float phaseTime;
        private void Update() {
            phaseTime = target.timeLeft;
        }
        public ISimData getTarget() {
            return target;
        }
    }
}