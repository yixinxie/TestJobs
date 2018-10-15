namespace Simulation_OOP {
    // 1 byte item id
    // 2 short  distance
    // 
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
    }

    public class InserterData : ISimData {
        public ushort expectedItemId;
        public float sourcePos;
        public float targetPos;
        public ISimData source, target;
        public float timeLeft;
        public byte phase; // 0: moving back to source, 1: moving towards target.
        public const float cycleDuration = 0.5f;
        public void wakeup() {
            if (source.attemptToRemove(expectedItemId, sourcePos)) {
                phase = 1;
                timeLeft = cycleDuration;
            }
        }
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
    }
}