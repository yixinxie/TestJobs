using UnityEngine;
public struct ConsumerData : IPipe
{
    public int[] objectCount;
    public int stackCount;
    public int headArrayIdx;
    public const int MaxItemCount = 64; // max item variety
    public void init(int idxHead)
    {
        objectCount = new int[MaxItemCount];
        for(int i = 0; i < MaxItemCount; ++i)
        {
            objectCount[i] = 0;
        }
        headArrayIdx = idxHead;
    }
    public bool attemptToTake(int itemId) {
        objectCount[itemId]++;
        Debug.LogFormat("{0} : {1}", itemId, objectCount[itemId]);
        return true;
    }
    public void block() {

    }
    public void unblock() {

    }
    public void pop() {

    }
}
