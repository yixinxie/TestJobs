using System.Collections;
using System.Collections.Generic;
namespace Simulation_OOP {
    public interface ISimView {
        ISimData getTarget();
    }
    public interface ISimData {

        bool attemptToInsert(ushort _itemId, float pos);
        bool attemptToRemove(ushort itemId, float atPos);
        //void wakeup();
    }
}