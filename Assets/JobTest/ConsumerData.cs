using UnityEngine;
public struct ConsumerData
{
    public int[] objectCount;
    public int stackCount;
    public int testint;
    public const int MaxItemCount = 64; // max item variety
    public void init(int _testint)
    {
        objectCount = new int[MaxItemCount];
        for(int i = 0; i < MaxItemCount; ++i)
        {
            objectCount[i] = 0;
        }
        testint = _testint;
    }
    public bool attemptToTake(int itemId) {
        objectCount[itemId]++;
        Debug.LogFormat("{0} : {1}", itemId, objectCount[itemId]);
        return true;
    }

}
