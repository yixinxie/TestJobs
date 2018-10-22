using UnityEngine;
namespace Simulation_OOP {
    public class SplitterData : ISimData {
        byte splitPolicy;
        ISimData source, output0, output1;
        public SplitterData() {
            splitPolicy = 0;
        }

        
        public bool attemptToInsert(ushort _itemId, float pos) {
            
            return false;
        }
        const float PickupDist = 0.2f;
        public bool attemptToRemove(ushort itemId, float atPos) {
            bool ret = false;
            return ret;
        }
        public void update(float dt) {
        }

        public void wakeup() {
        }

        public ISimData[] notifyArray = new ISimData[8];
        public void addNotify(ISimData target) {
            SimDataUtility.appendNotify(notifyArray, target);
        }
    }
}