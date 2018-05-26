public struct ConverterData
{
    public float speed;
    public int itemId;
    public int idxInUpdateArray; // index in tubeOutputUpdateData
    public ConverterData(int _itemId, float _speed)
    {
        speed = _speed;
        itemId = _itemId;
        idxInUpdateArray = -1;
    }

    
}
