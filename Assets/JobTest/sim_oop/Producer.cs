using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    
    public class Producer : MonoBehaviour, ISimView {
        public ProducerData target;

        public float timeLeft;
        public int current;
        private void Awake() {
            
        }
        public void initialize(ushort itemType, int remaining) {
            target.itemId = itemType;
            target.remaining = remaining;
        }
        private void Update() {
            timeLeft = target.getTimeLeft();
            current = target.count;
        }
        public ISimData getTarget() {
            return target;
        }
    }
}