
using UnityEngine;

namespace Simulation_OOP {
    // 1 byte item id
    // 2 short  distance
    // 
    public class SimDataUtility {
        public static void appendNotify(ISimData[] array, ISimData target) {
            for(int i = 0; i < array.Length; ++i) {
                if(array[i] == null) {
                    array[i] = target;
                    return;
                }
            }
            Debug.Log("not enough space for notify.");
        }

        public static void addPair(ISimData v0, ISimData v1) {
            v0.addNotify(v1);
            v1.addNotify(v0);
        }
    }
    public class InserterData_OOP : ISimData {
        public ushort expectedItemId;
        public float sourcePos;
        public float targetPos;
        public ISimData source, target;
        public float timeLeft;
        public byte phase; // 0: moving back to source, 1: moving towards target.
        public const float cycleDuration = 0.5f;
        public void update(float dt) {
            timeLeft -= dt;
            if (timeLeft <= 0.0f) {
                if (phase == 0 && source != null) {
                    // just reached the source.
                    if (source.attemptToRemove(expectedItemId, sourcePos)) {
                        phase = 1;
                        timeLeft = cycleDuration;
                    }
                }
                else if (phase == 1 && target != null) {
                    // just reached the target/destination
                    if (target.attemptToInsert(expectedItemId, targetPos)) {
                        phase = 0;
                        timeLeft = cycleDuration;
                    }
                }
            }
        }

        public bool attemptToInsert(ushort _itemId, float pos) {
            return false;
        }

        public bool attemptToRemove(ushort itemId, float atPos) {
            return false;
        }
        public void wakeup() {

        }

        public void addNotify(ISimData target) {
        }
    }

    public class InserterData : ISimData, IFloatUpdate {
        public ushort expectedItemId;
        public float sourcePos;
        public float targetPos;
        public ISimData source, target;
        public byte phase; // 0: moving back to source, 1: moving towards target.
        public const float cycleDuration = 0.5f;
        
        int floatUpdateHandle;
        public void wakeup() {
            bool added = FloatUpdate.self.getSubByIdx(floatUpdateHandle) == this;
            if (added) {
                float timeleft = FloatUpdate.self.getVal(floatUpdateHandle);
                if (timeleft > 0f) {
                    return;
                }
            }

            if (phase == 0 && source != null) {
                // just reached the source.
                if (source.attemptToRemove(expectedItemId, sourcePos)) {
                    FloatUpdate.self.Add(this, cycleDuration);
                    phase = 1;
                    //timeLeft = cycleDuration;
                    source.wakeup();
                }
            }
            else if (phase == 1 && target != null) {
                // just reached the target/destination
                if (target.attemptToInsert(expectedItemId, targetPos)) {
                    phase = 0;
                    target.wakeup();
                }
            }
        }

        public bool attemptToInsert(ushort _itemId, float pos) {
            return false;
        }

        public bool attemptToRemove(ushort itemId, float atPos) {
            return false;
        }

        public void NotifyIndexChange(int new_idx) {
            floatUpdateHandle = new_idx;
        }

        public void Zero(float left) {
            if (phase == 0 && source != null) {
                // just reached the source.
                if (source.attemptToRemove(expectedItemId, sourcePos)) {
                    phase = 1;
                    FloatUpdate.self.Add(this, cycleDuration);
                    for (int i = 0; i < notifyArray.Length; ++i) {
                        if (notifyArray[i] != null)
                            notifyArray[i].wakeup();
                    }
                }
            }
            else if (phase == 1 && target != null) {
                // just reached the target/destination
                if (target.attemptToInsert(expectedItemId, targetPos)) {
                    phase = 0;
                    FloatUpdate.self.Add(this, cycleDuration);
                    for (int i = 0; i < notifyArray.Length; ++i) {
                        if (notifyArray[i] != null)
                            notifyArray[i].wakeup();
                    }
                }
            }
        }
        public ISimData[] notifyArray = new ISimData[2];
        public void addNotify(ISimData target) {
            SimDataUtility.appendNotify(notifyArray, target);
        }
    }
}