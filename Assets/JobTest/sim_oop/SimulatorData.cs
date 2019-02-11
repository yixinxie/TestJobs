using System.Collections;
using System.Collections.Generic;
namespace Simulation_OOP {
    public interface ISimView {
        ISimData getTarget();
    }
    public interface ISimData {

        bool attemptToInsert(ushort _itemId);
        bool attemptToRemove(ushort itemId);
        void wakeup();
        void addNotify(ISimData target, float relativePos);
        void setTrafficState(byte newState);
    }
}