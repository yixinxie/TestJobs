using UnityEngine;
namespace Simulation_OOP {
    public class BeltData : ISimData {
        ushort[] itemIds;
        public float[] positions;
        float[] notifyPositions = new float[8];
        public int count;
        public float tubeLength;
        public float speed;
        public float itemHalfWidth;
        public const int Length = 10;

        public BeltData() {
            positions = new float[Length];
            itemIds = new ushort[Length];
            count = 0;
            tubeLength = 10f;
            speed = 1f;
            itemHalfWidth = 0.5f;
            for(int i = 0; i < notifyPositions.Length; ++i) {
                notifyPositions[i] = -1f;
            }
        }

        short canInsert(float pos) {
            short i = 0;
            if (count == 0) return 0;
            if (count == positions.Length) return -1;
            if (count > 0 && pos < positions[i] - 2 * itemHalfWidth) {
                return i;
            }

            for (; i < count - 1; ++i) {
                if (pos > positions[i] + itemHalfWidth * 2f && pos < positions[i + 1] - itemHalfWidth * 2f) {
                    return i;
                }
            }
            if (pos > positions[i] && pos < tubeLength - itemHalfWidth * 2f) {
                return i;
            }
            return -1;
        }

        public bool attemptToInsert(ushort _itemId, float pos) {
            bool ret = false;
            if (count > Length) return ret;

            int insertAt = canInsert(pos);
            if (insertAt >= 0) {

                for (int i = count; i > insertAt; --i) {
                    positions[i] = positions[i - 1];
                    itemIds[i] = itemIds[i - 1];
                    
                }
                positions[insertAt] = 0f;
                itemIds[insertAt] = _itemId;
                ret = true;
                count++;
            }
            return ret;
        }

        short queryItemAtPos(float pos, ushort itemId) {
            const float PickupDist = 0.2f;
            for (short i = 0; i < count; ++i) {
                if (Mathf.Abs(pos - positions[i]) < PickupDist && itemIds[i] == itemId) {
                    //Debug.Log("got " + i);
                    return i;
                }
            }
            return -1;
        }

        public bool attemptToRemove(ushort itemId, float atPos) {
            bool ret = false;
            short atIdx = queryItemAtPos(atPos, itemId);
            if (atIdx >= 0) {
                count--;
                for (int i = atIdx; i < count; ++i) {
                    positions[i] = positions[i + 1];
                    itemIds[i] = itemIds[i + 1];
                }
                ret = true;
            }
            return ret;
        }

        public void update(float dt) {
            int notifylength = notifyPositions.Length;
            for (int i = count - 1; i >= 0; --i) {
                positions[i] += dt * speed;
                if (i == count - 1) {
                    if (positions[i] > tubeLength) {
                        positions[i] = tubeLength;
                    }
                }
                else {
                    if (positions[i] > positions[i + 1] - itemHalfWidth * 2f) {
                        positions[i] = positions[i + 1] - itemHalfWidth * 2f;
                    }
                }
                for(int j = 0; j < notifylength; ++j) {
                    if (notifyPositions[j] < 0f) break;
                    float distToNotify = Mathf.Abs(notifyPositions[j] - positions[i]);
                    if(distToNotify < 0.2f) {
                        notifyArray[j].wakeup();
                    }
                }
            }
            //for(int i = 0; i < notifyArray.Length; ++i) {
            //    if(notifyArray[i] != null) {
            //        notifyArray[i].wakeup();
            //    }
            //}
        }

        public void wakeup() {
        }

        public ISimData[] notifyArray = new ISimData[8];
        
        public void addNotify(ISimData target, float relativePos) {
            //SimDataUtility.appendNotify(notifyArray, target);
            bool added = false;
            for (int i = 0; i < notifyArray.Length; ++i) {
                if (notifyArray[i] == null) {
                    notifyArray[i] = target;
                    notifyPositions[i] = relativePos;
                    added = true;
                    break;
                }
            }
            if(added == false)
                Debug.Log("not enough space for notify.");
        }

        public void setTrafficState(byte newState) {

        }
    }
}