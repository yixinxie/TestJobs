namespace Simulation_OOP {
    public class ProducerData : ISimData, IFloatUpdate {
        public ushort itemId;
        public int count; // produced
        public int remaining;
        public float cycleDuration;
        int floatUpdateHandle;
        public ProducerData() {
            cycleDuration = 2f;
        }
        public float getTimeLeft() {
            if (FloatUpdate.self.getSubByIdx(floatUpdateHandle) == this) {
                float timeleft = FloatUpdate.self.getVal(floatUpdateHandle);
                return timeleft;
            }
            return -1f;
        }
        public bool attemptToInsert(ushort _itemId) {
            return false;
        }

        public bool attemptToRemove(ushort itemId) {
            if (count > 0) {
                count--;
                return true;
            }
            return false;
        }

        public void NotifyIndexChange(int new_idx) {
            floatUpdateHandle = new_idx;
        }
        const int tempStorageCount = 3;
        public void wakeup() {
            bool added = FloatUpdate.self.getSubByIdx(floatUpdateHandle) == this;
            if (added) {
                float timeleft = FloatUpdate.self.getVal(floatUpdateHandle);
                if (timeleft > 0f) {
                    return;
                }
            }

            if (remaining > 0 && count < tempStorageCount) {
                if (added) {

                    FloatUpdate.self.SetVal(floatUpdateHandle, cycleDuration);
                }
                else {
                    FloatUpdate.self.Add(this, cycleDuration);
                }
            }
        }

        public void Zero(float left) {
            remaining--;
            count++;
            for (int i = 0; i < notifyArray.Length; ++i) {
                if (notifyArray[i] != null)
                    notifyArray[i].wakeup();
            }
            //wakeup();
            if (count < tempStorageCount && remaining > 0) {
                FloatUpdate.self.SetVal(floatUpdateHandle, cycleDuration);
            }
            else {
                FloatUpdate.self.RemoveAt(floatUpdateHandle);
            }
        }

        public ISimData[] notifyArray = new ISimData[2];
        public void addNotify(ISimData target) {
            SimDataUtility.appendNotify(notifyArray, target);
        }
    }
}