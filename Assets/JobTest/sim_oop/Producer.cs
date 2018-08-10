using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public class Producer : MonoBehaviour {
        public ProducerData target;

        public float timeLeft;
        private void Update() {
            timeLeft = target.timeLeft;
        }
    }
}