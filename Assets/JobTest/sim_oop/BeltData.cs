using UnityEngine;
namespace Simulation_OOP {
    public class BeltData : ISimData {
        ushort[] itemIds;
        public float[] positions;

        public int count;
        public float tubeLength;
        public float speed;
        public float itemHalfWidth;
        public const int defaultLength = 10;

        public BeltData() {
            positions = new float[defaultLength];
            itemIds = new ushort[defaultLength];
            count = 0;
            tubeLength = 10f;
            speed = 1f;
            itemHalfWidth = 0.5f;
        }

        //short canInsert(float pos) {
        //    short i = 0;
        //    if (count == 0) return 0;
        //    if (count == positions.Length) return -1;
        //    if (count > 0 && pos < positions[i] - 2 * itemHalfWidth) {
        //        return i;
        //    }

        //    for (; i < count - 1; ++i) {
        //        if (pos > positions[i] + itemHalfWidth * 2f && pos < positions[i + 1] - itemHalfWidth * 2f) {
        //            return i;
        //        }
        //    }
        //    if (pos > positions[i] && pos < tubeLength - itemHalfWidth * 2f) {
        //        return i;
        //    }
        //    return -1;
        //}
        public bool canInsert() {
            short i = 0;
            if (count == 0) return true;
            if (count == positions.Length) return false;

            return 2f * itemHalfWidth < positions[i];
        }
        public bool attemptToInsert(ushort _itemId) {
            bool ret = false;
            if (count > defaultLength) return ret;

            if (canInsert()) {

                for (int i = count; i > 0; --i) {
                    positions[i] = positions[i - 1];
                    itemIds[i] = itemIds[i - 1];
                    
                }
                positions[0] = 0f;
                itemIds[0] = _itemId;
                ret = true;
                count++;
            }
            return ret;
        }
        //public bool attemptToInsert(ushort _itemId, float pos) {
        //    bool ret = false;
        //    if (count > Length) return ret;

        //    int insertAt = canInsert(pos);
        //    if (insertAt >= 0) {

        //        for (int i = count; i > insertAt; --i) {
        //            positions[i] = positions[i - 1];
        //            itemIds[i] = itemIds[i - 1];

        //        }
        //        positions[insertAt] = 0f;
        //        itemIds[insertAt] = _itemId;
        //        ret = true;
        //        count++;
        //    }
        //    return ret;
        //}
        const float PickupDist = 0.2f;
        short queryItemAtPos(float pos, ushort itemId) {
            
            for (short i = 0; i < count; ++i) {
                if (Mathf.Abs(pos - positions[i]) < PickupDist && itemIds[i] == itemId) {
                    //Debug.Log("got " + i);
                    return i;
                }
            }
            return -1;
        }
        //public bool attemptToRemove(ushort itemId, float atPos) {
        //    bool ret = false;
        //    short atIdx = queryItemAtPos(atPos, itemId);
        //    if (atIdx >= 0) {
        //        count--;
        //        for (int i = atIdx; i < count; ++i) {
        //            positions[i] = positions[i + 1];
        //            itemIds[i] = itemIds[i + 1];
        //        }
        //        ret = true;
        //    }
        //    return ret;
        //}
        public bool attemptToRemove(ushort itemId) {
            bool ret = false;

            if (canRemove()) {
                count--;
                ret = true;
            }
            return ret;
        }
        public bool canRemove() {
            if (count == 0) return false;
            return tubeLength - positions[count - 1] < PickupDist;
        }
        public void update(float dt) {
            int lastIdx = count - 1;
            for (int i = lastIdx; i >= 0; --i) {
                positions[i] += dt * speed;
                if (i == lastIdx) {
                    if (positions[i] > tubeLength) {
                        positions[i] = tubeLength;
                    }
                }
                else {
                    if (positions[i] > positions[i + 1] - itemHalfWidth * 2f) {
                        positions[i] = positions[i + 1] - itemHalfWidth * 2f;
                    }
                }
            }
            if (count > 0 && positions[0] > itemHalfWidth) {
                notifyArray[0].wakeup();
            }
            if(count > 0 && positions[lastIdx] > tubeLength - PickupDist) {
                notifyArray[1].wakeup();
            }
        }

        public void wakeup() {
        }

        public ISimData[] notifyArray = new ISimData[8];
        public void addNotify(ISimData target) {
            SimDataUtility.appendNotify(notifyArray, target);
        }
    }
}