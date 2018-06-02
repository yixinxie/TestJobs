public struct GeneratorData
{
    public int remaining;
    public float timeToMakeOne;
    public int itemId;
    public int idxInUpdateArray; // index in generalUpdateData
    public int idxInOutputStateArray;
    public void init(int _itemId, float _speed, int _remaining, int idx, int outputIdx)
    {
        remaining = _remaining;
        timeToMakeOne = _speed;
        itemId = _itemId;
        idxInUpdateArray = TubeSimulate.generic[0].addEntity();
        idxInOutputStateArray = outputIdx;
    }

    public void start()
    {
        GenericUpdateData d = TubeSimulate.generic[0].genericUpdateData[idxInUpdateArray];
        d.timeLeft = timeToMakeOne;
        TubeSimulate.generic[0].genericUpdateData[idxInUpdateArray] = d;
    }
    public void onExpended()
    {
        remaining--;
        if(remaining > 0)
        {
            start();
        }
    }
}
