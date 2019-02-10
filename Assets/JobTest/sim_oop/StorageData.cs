namespace Simulation_OOP {
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

        public void wakeup() {
        }
        public void setTrafficState(byte newState) {

        }

        public void addNotify(ISimData target, float relativePos) {
        }
    }
}