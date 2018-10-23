using UnityEngine;
namespace Simulation_OOP {
    public class MergerData : ISimData {
        byte mergePolicy;
        int alternate;
        ushort expectedItem;
        float sourcePos;
        BeltData currentInput;
        BeltData output;
        BeltData[] inputs;

        public MergerData() {
            mergePolicy = 0;
            inputs = new BeltData[2];
        }

        
        public bool attemptToInsert(ushort _itemId) {
            return false;
        }
        
        public bool attemptToRemove(ushort itemId) {
            return false;
        }

        public void wakeup() {
            if (output != null && currentInput != null) {
                if (output.canInsert() && currentInput.canRemove()) {
                    output.attemptToRemove(expectedItem);
                    currentInput.attemptToInsert(expectedItem);
                    alternate++;
                    alternate %= inputs.Length;
                    currentInput = inputs[alternate];

                }
            }
        }
        //public void update(float dt) {
            
        //}

        //public ISimData[] notifyArray = new ISimData[8];
        public void addNotify(ISimData target) {
            //SimDataUtility.appendNotify(notifyArray, target);
        }
    }
}