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
        public void initialize(ResourceNode node) {
            target.itemId = node.itemType;
            target.remaining = node.remaining;
        }
        private void Update() {
            timeLeft = target.timeLeft;
            current = target.count;
        }
        public ISimData getTarget() {
            return target;
        }
    }
}