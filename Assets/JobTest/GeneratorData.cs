public struct GeneratorData
{
    public int remaining;
    public float timeToMakeOne;
    public int itemId;
    public int idxInUpdateArray; // index in generalUpdateData
    public void init(int _itemId, float _speed, int _remaining, int idx)
    {
        remaining = _remaining;
        timeToMakeOne = _speed;
        itemId = _itemId;
        idxInUpdateArray = idx;
    }

    private void transfer()
    {
        GeneralUpdateData d = Ref.self.generalUpdateData[idxInUpdateArray];
        d.timeLeft = timeToMakeOne;
        Ref.self.generalUpdateData[idxInUpdateArray] = d;
    }
    public void start()
    {
        transfer();
    }
    public void onExpended()
    {

        remaining--;
        if(remaining > 0)
        {
            transfer();
        }

    }


}
