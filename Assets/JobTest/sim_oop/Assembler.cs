using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation_OOP {
    public class Assembler : MonoBehaviour, ISimView {
        public AssemblerData target;
        public int totalProduced;
        public float left;
        public ushort[] inventory;
        public void Update() {
            left = target.getTimeLeft();
            totalProduced = target.totalProduced;
            inventory = target.currentCount;
        }
        public ISimData getTarget() {
            return target;
        }
    }
    

}