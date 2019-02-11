namespace Simulation_OOP {
    public class AssemblerData : ISimData, IFloatUpdate {
        public ushort[] req_itemIds;
        public ushort[] req_Count;
        public ushort[] currentCount; // frequently changes
        public ushort productItemId;
        public ushort productItemCount; // frequently changes
        public float cycleDuration;
        public int totalProduced;
        public ushort itemCap;
        public AssemblerData(){
            req_itemIds = new ushort[3];
            req_Count = new ushort[3];
            currentCount = new ushort[3];
        }
        public float getTimeLeft() {
            if (FloatUpdate.self.getSubByIdx(floatUpdateHandle) == this) {
                float timeleft = FloatUpdate.self.getVal(floatUpdateHandle);
                return timeleft;
            }
            return -1f;
        }
        public void setReqItems(ushort[] ids, ushort[] counts) {
            for(int i = 0; i < req_itemIds.Length; ++i) {
                if (i >= ids.Length) break;
                req_itemIds[i] = ids[i];
                req_Count[i] = counts[i];
            }
            itemCap = req_Count[0];
            itemCap *= 2;
        }

        public bool attemptToInsert(ushort _itemId) {
            bool inserted = false;
            for(int i = 0; i < req_itemIds.Length; ++i) {
                if(req_itemIds[i] == _itemId && currentCount[i] < itemCap) {
                    currentCount[i]++;
                    inserted = true;
                    break;
                }
            }
            wakeup();
            return inserted;
        }


        public bool attemptToRemove(ushort itemId) {
            if(productItemCount > 0 && itemId == productItemId) {
                productItemCount--;
                return true;
            }
            return false;
        }

        public void wakeup() {
            
            bool added = FloatUpdate.self.getSubByIdx(floatUpdateHandle) == this;
            if (added) {
                float timeleft = FloatUpdate.self.getVal(floatUpdateHandle);
                if (timeleft > 0f) {
                    return;
                }
            }
            bool allMet = true;
            for (int i = 0; i < req_itemIds.Length; ++i) {
                if (currentCount[i] < req_Count[i] && req_Count[i] > 0) {
                    allMet = false;
                    break;
                }
            }
            if (allMet) {
                for (int i = 0; i < req_itemIds.Length; ++i) {
                    currentCount[i] -= req_Count[i];
                }
                FloatUpdate.self.Add(this, cycleDuration);
            }
        }
        int floatUpdateHandle;
        public void NotifyIndexChange(int new_idx) {
            floatUpdateHandle = new_idx;
        }

        public void Zero(float left) {
            productItemCount++;
            totalProduced++;

            for (int i = 0; i < notifyArray.Length; ++i) {
                if (notifyArray[i] != null)
                    notifyArray[i].wakeup();
            }

            bool allMet = true;
            for (int i = 0; i < req_itemIds.Length; ++i) {
                if (currentCount[i] < req_Count[i] && req_Count[i] > 0) {
                    allMet = false;
                    break;
                }
            }
            if (allMet) {
                for (int i = 0; i < req_itemIds.Length; ++i) {
                    currentCount[i] -= req_Count[i];
                }
                FloatUpdate.self.SetVal(floatUpdateHandle, cycleDuration);
            }
            else {
                FloatUpdate.self.RemoveAt(floatUpdateHandle);
            }

        }

        public ISimData[] notifyArray = new ISimData[4];
        public void addNotify(ISimData target, float relativePos) {
            SimDataUtility.appendNotify(notifyArray, target);
        }

        public void setTrafficState(byte newState) {

        }
    }
}