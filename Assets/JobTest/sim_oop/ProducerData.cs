namespace Simulation_OOP {
    public class ProducerData : ISimData, IFloatUpdate {
        public float timeLeft;
        public ushort itemId;
        public int count; // produced
        public int remaining;
        public float cycleDuration;
        int floatUpdateHandle;
        public ProducerData() {
            cycleDuration = 2f;
        }

        public bool attemptToInsert(ushort _itemId, float pos) {
            return false;
        }

        public bool attemptToRemove(ushort itemId, float atPos) {
            if (count > 0) {
                count--;
                return true;
            }
            return false;
        }

        public void NotifyIndexChange(int new_idx) {
            floatUpdateHandle = new_idx;
        }

        public void wakeup() {
            bool added = FloatUpdate.self.getSubByIdx(floatUpdateHandle) == this;
            if (added) {
                float timeleft = FloatUpdate.self.getVal(floatUpdateHandle);
                if (timeleft > 0f) {
                    return;
                }
            }

            if (remaining > 0) {
                FloatUpdate.self.Add(this, cycleDuration);
            }
        }

        public void Zero(float left) {
            remaining--;
            count++;
            for (int i = 0; i < notifyArray.Length; ++i) {
                if (notifyArray[i] != null)
                    notifyArray[i].wakeup();
            }
            wakeup();
        }

        public ISimData[] notifyArray = new ISimData[2];
        public void addNotify(ISimData target) {
            SimDataUtility.appendNotify(notifyArray, target);
        }
    }
}