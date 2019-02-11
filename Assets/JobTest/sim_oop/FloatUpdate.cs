using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace Simulation_OOP {
    public interface IFloatUpdate {
        void NotifyIndexChange(int new_idx);
        void Zero(float left);
    }

    public interface IByteUpdate {
        void NotifyIndexChange(int new_idx);
        void Zero();
    }


    public class FloatUpdate : IUpdate {
        public static FloatUpdate self;
        float[] values;
        IFloatUpdate[] subs;
        int count;
        struct FloatUpdateAddCmd {
            public float val;
            public IFloatUpdate target;
        }
        bool isUpdating;
        List<FloatUpdateAddCmd> toAdd = new List<FloatUpdateAddCmd>();
        public FloatUpdate(int defaultLength = 16) {
            values = new float[defaultLength];
            subs = new IFloatUpdate[defaultLength];
            self = this;
        }
        void AddDirect(IFloatUpdate sub, float val) {
            if (count >= values.Length) {
                float[] newArray = new float[values.Length * 2];
                IFloatUpdate[] newSubsArray = new IFloatUpdate[subs.Length * 2];
                Array.Copy(values, 0, newArray, 0, values.Length);
                Array.Copy(subs, 0, newSubsArray, 0, values.Length);
                values = newArray;
                subs = newSubsArray;
                //Debug.Log("alloc");
            }

            values[count] = val;
            subs[count] = sub;
            sub.NotifyIndexChange(count);
            count++;
        }
        
        public void Add(IFloatUpdate sub, float val) {
            if (isUpdating) {
                FloatUpdateAddCmd cmd;
                cmd.val = val;
                cmd.target = sub;
                toAdd.Add(cmd);
            }
            else {
                AddDirect(sub, val);
            }
        }
        public float getVal(int idx) {
            return values[idx];
        }
        public void SetVal(int idx, float newVal) {
            values[idx] = newVal;
        }
        public IFloatUpdate getSubByIdx(int idx) {
            return subs[idx];
        }
        public void RemoveAt(int idx) {
            if (isUpdating) {
                toRemove.Add(idx);
            }
            else {
                RemoveAtDirect(idx);
            }
        }
        public void RemoveAtDirect(int idx) {
            count--;
            values[idx] = values[count];
            subs[idx] = subs[count];
            subs[idx].NotifyIndexChange(idx);
        }
        List<int> toRemove = new List<int>();
        public void PerFrameUpdate(float dt) {
            Profiler.BeginSample("float update");
            isUpdating = true;
            for(int i = 0; i < toAdd.Count; ++i) {
                AddDirect(toAdd[i].target, toAdd[i].val);
            }
            toAdd.Clear();
            float _count = count;
            for(int i = 0; i < _count; ++i) {
                values[i] -= dt;
                if(values[i] <= 0.0f) {
                    subs[i].Zero(values[i]);
                    //toRemove.Add(i);
                }
            }
            for(int i = toRemove.Count - 1; i >= 0; --i) {
                RemoveAtDirect(toRemove[i]);
            }
            toRemove.Clear();
            isUpdating = false;
            Profiler.EndSample();
        }
        public int getCount() {
            return count;
        }
        
    }


    public class ByteChecker : IUpdate {
        public static ByteChecker self;
        byte[] values;
        IByteUpdate[] subs;
        int count;
        struct ByteAddCmd {
            public byte val;
            public IByteUpdate target;
        }
        bool isUpdating;
        List<ByteAddCmd> toAdd = new List<ByteAddCmd>();
        public ByteChecker(int defaultLength = 16) {
            values = new byte[defaultLength];
            subs = new IByteUpdate[defaultLength];
            self = this;
        }
        void AddDirect(IByteUpdate sub, byte val) {
            if (count >= values.Length) {
                byte[] newArray = new byte[values.Length * 2];
                IByteUpdate[] newSubsArray = new IByteUpdate[subs.Length * 2];
                Array.Copy(values, 0, newArray, 0, values.Length);
                Array.Copy(subs, 0, newSubsArray, 0, values.Length);
                values = newArray;
                subs = newSubsArray;
                //Debug.Log("alloc");
            }

            values[count] = val;
            subs[count] = sub;
            sub.NotifyIndexChange(count);
            count++;
        }

        public void Add(IByteUpdate sub, byte val) {
            if (isUpdating) {
                ByteAddCmd cmd;
                cmd.val = val;
                cmd.target = sub;
                toAdd.Add(cmd);
            }
            else {
                AddDirect(sub, val);
            }
        }
        public float getVal(int idx) {
            return values[idx];
        }
        public void SetVal(int idx, byte newVal) {
            values[idx] = newVal;
        }
        public IByteUpdate getSubByIdx(int idx) {
            return subs[idx];
        }
        public void RemoveAt(int idx) {
            if (isUpdating) {
                toRemove.Add(idx);
            }
            else {
                RemoveAtDirect(idx);
            }
        }
        public void RemoveAtDirect(int idx) {
            count--;
            values[idx] = values[count];
            subs[idx] = subs[count];
            subs[idx].NotifyIndexChange(idx);
        }
        List<int> toRemove = new List<int>();
        public void PerFrameUpdate(float dt) {
            Profiler.BeginSample("byte update");
            isUpdating = true;
            for (int i = 0; i < toAdd.Count; ++i) {
                AddDirect(toAdd[i].target, toAdd[i].val);
            }
            toAdd.Clear();
            float _count = count;
            for (int i = 0; i < _count; ++i) {
                //values[i] -= dt;

                if (values[i] == 0) {
                    subs[i].Zero();
                    //toRemove.Add(i);
                }
            }
            for (int i = toRemove.Count - 1; i >= 0; --i) {
                RemoveAtDirect(toRemove[i]);
            }
            toRemove.Clear();
            isUpdating = false;
            Profiler.EndSample();
        }
        public int getCount() {
            return count;
        }

    }
}