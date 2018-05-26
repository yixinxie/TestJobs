public struct ConsumerData
{
    public int[] objectCount;
    public int stackCount;
    public const int MaxItemCount = 64; // max item variety
    public void init(int _itemId, int idx)
    {
        objectCount = new int[MaxItemCount];
        for(int i = 0; i < MaxItemCount; ++i)
        {
            objectCount[i] = 0;
        }
    }

}
