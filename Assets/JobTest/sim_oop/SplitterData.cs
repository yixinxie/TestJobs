using UnityEngine;
namespace Simulation_OOP {
    public class SplitterData : ISimData {
        byte splitPolicy;
        int alternate;
        ushort expectedItem;
        float sourcePos;
        BeltData currentOutput;
        BeltData source;
        BeltData[] outputs;

        public SplitterData() {
            splitPolicy = 0;
            outputs = new BeltData[2];
        }

        
        public bool attemptToInsert(ushort _itemId) {
            return false;
        }
        
        public bool attemptToRemove(ushort itemId) {
            return false;
        }

        public void wakeup() {
            if (source != null && currentOutput != null) {
                if (source.canRemove() && currentOutput.canInsert()) {
                    source.attemptToRemove(expectedItem);
                    currentOutput.attemptToInsert(expectedItem);

                    alternate++;
                    alternate %= outputs.Length;
                    currentOutput = outputs[alternate];

                }
            }
        }
        //public void update(float dt) {
            
        //}

        public ISimData[] notifyArray = new ISimData[8];
        public void addNotify(ISimData target) {
            SimDataUtility.appendNotify(notifyArray, target);
        }
    }
}