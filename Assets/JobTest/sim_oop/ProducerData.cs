namespace Simulation_OOP {
    public class ProducerData : ISimData {
        public float timeLeft;
        public ushort itemId;
        public int count; // produced
        public int remaining;
        public float cycleDuration;
        
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
                remaining--;
                count++;
            }
        }
    }
}