using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public class Assembler : MonoBehaviour, ISimView {
        public AssemblerData target;
        public ISimData getTarget() {
            return target;
        }
    }

}