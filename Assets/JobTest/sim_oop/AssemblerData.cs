namespace Simulation_OOP {
    public class AssemblerData : ISimData {
        public ushort[] req_itemIds;
        public ushort[] req_Count;
        public ushort[] currentCount; // frequently changes
        public ushort productItemId;
        public ushort productItemCount; // frequently changes
        public float cycleDuration;
        public float timeLeft;  // frequently changes
        public int totalProduced;
        public ushort itemCap;
        public AssemblerData(){
            req_itemIds = new ushort[3];
            req_Count = new ushort[3];
            currentCount = new ushort[3];
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

        public bool attemptToInsert(ushort _itemId, float pos) {
            bool inserted = false;
            for(int i = 0; i < req_itemIds.Length; ++i) {
                if(req_itemIds[i] == _itemId && currentCount[i] < itemCap) {
                    currentCount[i]++;
                    inserted = true;
                    break;
                }
            }
            checkForStart();
            return inserted;
        }
        bool checkForStart() {
            if(timeLeft > 0f) {
                return false;
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
                timeLeft = cycleDuration;
            }
            return allMet;
        }

        public bool attemptToRemove(ushort itemId, float atPos) {
            if(productItemCount > 0 && itemId == productItemId) {
                productItemCount--;
                return true;
            }
            return false;
        }
        public void update(float dt) {
            if (timeLeft <= 0f) return;
            timeLeft -= dt;
            if (timeLeft <= 0.0f) {
                productItemCount++;
                totalProduced++;
                checkForStart();
                

            }
        }
    }
}