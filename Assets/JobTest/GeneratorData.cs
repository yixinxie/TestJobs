using UnityEngine;
public interface IPipe {
    void block();
    void unblock();
    void pop();
}
public struct GeneratorData : IPipe
{
    public int remaining;
    public float timeToMakeOne;
    public ushort itemId;
    public int idxInUpdateArray; // index in generalUpdateData
    public int idxInEndStateArray;
    public void init(ushort _itemId, float _speed, int _remaining, int idx, int outputIdx)
    {
        remaining = _remaining;
        timeToMakeOne = _speed;
        itemId = _itemId;
        idxInUpdateArray = TubeSimulate.generic[0].addEntity();
        idxInEndStateArray = outputIdx;
    }

    public void start()
    {
        GenericUpdateData d = TubeSimulate.generic[0].genericUpdateData[idxInUpdateArray];
        d.timeLeft = timeToMakeOne;
        TubeSimulate.generic[0].genericUpdateData[idxInUpdateArray] = d;
    }
    public void pop()
    {
        remaining--;
        if(remaining > 0)
        {
            start();
        }
    }
    public void block() {

    }
    public void unblock() {
        GenericUpdateData d = TubeSimulate.generic[0].genericUpdateData[idxInUpdateArray];
        if(d.timeLeft <= 0.0f)
            start();
    }
    // visual related
    public Vector3 pos;
}
