using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public class Inserter : MonoBehaviour {
        public InserterData target;
        public float phaseTime;
        private void Update() {
            phaseTime = target.timeLeft;
        }
    }
}