using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    
    public class Producer : MonoBehaviour, ISimView {
        public ProducerData target;

        public float timeLeft;
        private void Awake() {
            
        }
        public void initialize(ResourceNode node) {
            target.itemId = node.itemType;
            target.count = node.remaining;
        }
        private void Update() {
            timeLeft = target.timeLeft;
        }
        public ISimData getTarget() {
            return target;
        }
    }
}