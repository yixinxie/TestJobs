﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public interface ISimData {

        bool attemptToInsert(ushort _itemId, float pos);
        bool attemptToRemove(ushort itemId, float atPos);
    }
    public class BeltData : ISimData {
        public ushort[] itemIds;
        public float[] positions;

        public int count;
        public float physicalLength;
        public float speed;
        public float itemHalfWidth;
        public const int Length = 10;
        public BeltData() {
            positions = new float[Length];
            itemIds = new ushort[Length];
            count = 0;
            physicalLength = 10f;
        }

        short canInsert(float pos) {
            short i = 0;
            if (count == 0) return 0;
            if (count > 0 && pos < positions[i] - 2 * itemHalfWidth) {
                return i;
            }

            for (; i < count - 1; ++i) {
                if (pos > positions[i] + itemHalfWidth * 2f && pos < positions[i + 1] - itemHalfWidth * 2f) {
                    return i;
                }
            }
            if (pos > positions[i] && pos < physicalLength - itemHalfWidth * 2f) {
                return i;
            }
            return -1;
        }
        public bool attemptToInsert(ushort _itemId, float pos) {
            bool ret = false;
            if (count > Length) return ret;


            int insertAt = canInsert(pos);
            if (insertAt >= 0) {
                for (int i = 0; i < count; ++i) {
                    positions[i + 1] = positions[i];
                    itemIds[i + 1] = itemIds[i];
                    ret = true;
                }
                count++;
            }
            return ret;
        }

        short queryItemAtPos(float pos, ushort itemId) {
            const float PickupDist = 0.2f;
            for (short i = 0; i < count; ++i) {
                if (Mathf.Abs(pos - positions[i]) < PickupDist && itemIds[i] == itemId) {
                    return i;
                }
            }
            return -1;
        }
        public bool attemptToRemove(ushort itemId, float atPos) {
            bool ret = false;
            short atIdx = queryItemAtPos(atPos, itemId);
            if (atIdx >= 0) {
                for (int i = atIdx; i < count; ++i) {
                    positions[i + 1] = positions[i];
                    itemIds[i + 1] = itemIds[i];
                }
                count--;
                ret = true;
            }
            return ret;
        }
        public void update(float dt) {
            for (int i = count - 1; i >= 0; --i) {
                positions[i] += dt * speed;
                if (i == count - 1) {
                    if (positions[i] > physicalLength) {
                        positions[i] = physicalLength;
                    }
                }
                else {
                    if (positions[i] > positions[i + 1] - itemHalfWidth * 2f) {
                        positions[i] = positions[i + 1] - itemHalfWidth * 2f;
                    }
                }
            }
        }
    }
    public class InserterData {
        public ushort expectedItemId;
        public float sourcePos;
        public float targetPos;
        public ISimData source, target;
        public float timeLeft;
        public byte phase; // 0: empty, 1: moving stuff.
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
    }
    public class ProducerData : ISimData {
        public ushort itemId;
        public int count;
        public float cycleDuration;
        public float timeLeft;
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
        public void update(float dt) {
            timeLeft -= dt;
            if (timeLeft <= 0.0f) {
                timeLeft += cycleDuration;
                count++;
            }
        }
    }
    public class StorageData : ISimData {
        public short[] stacks;
        public const int MaxItemTypeCount = 10;
        public StorageData() {
            stacks = new short[MaxItemTypeCount];
        }
        public bool attemptToInsert(ushort itemId, float pos) {
            stacks[itemId]++;
            return true;
        }

        public bool attemptToRemove(ushort itemId, float atPos) {
            if (stacks[itemId] > 0) {
                stacks[itemId]--;
                return true;
            }
            return false;
        }
    }
    public class AssemblerData : ISimData {
        public ushort[] req_itemIds;
        public ushort[] req_Count;
        public ushort[] currentCount; // frequently changes
        public ushort productItemId;
        public ushort productItemCount; // frequently changes
        public float cycleDuration;
        public float timeLeft;  // frequently changes
        public AssemblerData(){
            req_itemIds = new ushort[3];
            req_Count = new ushort[3];
            currentCount = new ushort[3];
        }

        public bool attemptToInsert(ushort _itemId, float pos) {
            bool ret = false;
            for(int i = 0; i < req_itemIds.Length; ++i) {
                if(req_itemIds[i] == _itemId && currentCount[i] < req_Count[i]) {
                    currentCount[i]++;
                    ret = true;
                    break;
                }
            }
            checkForStart();
            return ret;
        }
        bool checkForStart() {
            bool allMet = true;
            for (int i = 0; i < req_itemIds.Length; ++i) {
                if (currentCount[i] == req_Count[i]) {
                    allMet = false;
                }
            }
            if (allMet && timeLeft == 0f) {
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
            timeLeft -= dt;
            if (timeLeft <= 0.0f) {
                productItemCount++;
                checkForStart();
                
            }
        }
    }
}