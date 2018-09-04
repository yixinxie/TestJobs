//#define Disable_Check_Counting
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Pathea.HotAreaNs {
    public interface IAgent {
        float GetInterval();
        Vector3 GetPos();
        float GetDistanceThreshold();
        void Distance(float distance);
    }
    public class HotAreaModule : MonoBehaviour {
        const int DefaultItemLength = 1024;
        float[] itemNextCheckTime = new float[DefaultItemLength];
        IAgent[] itemAgents = new IAgent[DefaultItemLength];
        int itemCount = 0;

        float referenceMovementSpeed = 8f;
        float timeCoefficient = 1f;
        private Vector3 hotPos;

        const int PositionHistoryLength = 50;
        Vector3[] positionHistory = new Vector3[PositionHistoryLength];
        float[] positionTimes = new float[PositionHistoryLength];
        int position_begin_ptr;
        int position_count;
        float elapsedTime;
        float lastResetTime;
        List<int> todoList = new List<int>(100);

#if UNITY_EDITOR || CCC_CMD
        private bool zeroDistance = false;

        public bool IsZeroDistance() {
            return zeroDistance;
        }

        public void ToggleZeroDistance() {
            zeroDistance = !zeroDistance;
        }
#endif
#if UNITY_EDITOR && !Disable_Check_Counting
        int[] runs = new int[100];
        int run_ptr;
#endif
        public static HotAreaModule self;
        private void Awake() {
            self = this;
        }
        protected void Update() {
            SetHotPos(transform.position, Time.deltaTime);
            Profiler.BeginSample("HotArea Eval");
            float deltaTime = Time.deltaTime * timeCoefficient;
            for (int i = 0; i < itemCount; i++) {
#if UNITY_EDITOR || CCC_CMD
                if (zeroDistance) {

                    itemAgents[i].Distance(Mathf.Epsilon);
                }
                else {
#endif
                    itemNextCheckTime[i] -= deltaTime;
                    if (itemNextCheckTime[i] <= 0f) {
                        todoList.Add(i);
                    }
#if UNITY_EDITOR || CCC_CMD
                }
#endif

            }
            Profiler.EndSample();
            Profiler.BeginSample("HotArea Action");
            Vector3 cachedPos = hotPos;
            float moveSpeed = referenceMovementSpeed;
            for (int j = 0; j < todoList.Count; ++j) {
                int i = todoList[j];
                Vector3 offset = cachedPos - itemAgents[i].GetPos();
                float distance = offset.magnitude;
                float distanceThreshold = itemAgents[i].GetDistanceThreshold();
                float minTimeToGetInRange = 0f;
                itemAgents[i].Distance(distance);
                if (distance > distanceThreshold) {
                    minTimeToGetInRange = (distance - distanceThreshold) / moveSpeed;
                }
                itemNextCheckTime[i] = (itemAgents[i].GetInterval() + minTimeToGetInRange) * UnityEngine.Random.Range(0.9f, 1f);
            }

#if UNITY_EDITOR && !Disable_Check_Counting
            runs[run_ptr] = todoList.Count;
            run_ptr++;
            run_ptr %= runs.Length;
            if (run_ptr == 0) {
                int sum = 0;
                for (int i = 0; i < runs.Length; ++i) {
                    sum += runs[i];
                }
                Debug.Log("count in 100 frames: " + sum);
            }
#endif
            todoList.Clear();
            Profiler.EndSample();
        }
        // call this when the player has traveled a long distance in a very short time. i.e. teleport
        // short range teleport does not need to call this.
        void ResetIntervals() {
            for (int i = 0; i < itemCount; i++) {
                itemNextCheckTime[i] = itemAgents[i].GetInterval() / 2f * UnityEngine.Random.Range(0.9f, 1f);
            }
        }

        public void Add(IAgent agent) {
            if (agent == null || agent.Equals(null)) {
                Debug.LogError("hot area agent cant be null.");
                return;
            }
            if (itemCount >= itemAgents.Length) {
                float[] newFloatArray = new float[itemAgents.Length * 2];
                IAgent[] newAgentArray = new IAgent[itemAgents.Length * 2];
                Array.Copy(itemNextCheckTime, 0, newFloatArray, 0, itemNextCheckTime.Length);
                Array.Copy(itemAgents, 0, newAgentArray, 0, itemAgents.Length);
                itemNextCheckTime = newFloatArray;
                itemAgents = newAgentArray;
            }
            itemAgents[itemCount] = agent;
            itemNextCheckTime[itemCount] = 0.0f;
            itemCount++;
        }

        public void Remove(IAgent agent) {
            for (int i = 0; i < itemCount; ++i) {
                if (itemAgents[i] == agent) {
                    itemAgents[i] = itemAgents[itemCount - 1];
                    itemNextCheckTime[i] = itemNextCheckTime[itemCount - 1];
                    itemCount--;
                    break;
                }
            }
        }

        public void SetHotPos(Vector3 _hotPos, float dt) {
            hotPos = _hotPos;
            elapsedTime += dt;

            // use a cyclic buffer to keep track of the times and positions
            int update_idx = position_begin_ptr + position_count;
            update_idx %= PositionHistoryLength;
            positionHistory[update_idx] = hotPos;
            positionTimes[update_idx] = elapsedTime;
            if (position_count < PositionHistoryLength) {
                position_count++;
            }
            else {
                position_begin_ptr++;
                if (position_begin_ptr >= PositionHistoryLength) {
                    position_begin_ptr %= PositionHistoryLength;
                }
            }
            Vector3 lastPos = Vector3.zero;
            float lastTime = 0f;
            for (int i = 0; i < position_count; ++i) {
                int idx = position_begin_ptr + i;
                idx %= PositionHistoryLength;
                if (positionTimes[idx] > elapsedTime - 0.5f) {
                    lastTime = positionTimes[idx];
                    lastPos = positionHistory[idx];
                    break;
                }
            }

            float dist = Vector3.Distance(lastPos, hotPos);
            float speed = 0f;
            if (Mathf.Abs(elapsedTime - lastTime) > Mathf.Epsilon) {
                speed = dist / (elapsedTime - lastTime);
            }
            timeCoefficient = speed / referenceMovementSpeed;
            if (timeCoefficient < 1f) timeCoefficient = 1f;

            // do not reset the intervals too frequently, as it incurs a huge cost.
            if (speed > referenceMovementSpeed * 10f && elapsedTime - lastResetTime > 1f) {
                Debug.Log("reset interval at " + speed);
                ResetIntervals();
                lastResetTime = elapsedTime;
            }
        }
    }

}