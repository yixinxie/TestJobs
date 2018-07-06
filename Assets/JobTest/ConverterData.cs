using UnityEngine;

public struct ConverterData : IPipe
{
    public ushort[] srcIds;
    public byte[] srcRequired; // requirement
    public ushort targetId;
    public byte targetCount;

    public byte[] srcCurrent; // current count

    public float timeToMakeOne;
    public int idxInUpdateArray; // index in tubeOutputUpdateData
    public int idxHeadInEndStateArray;
    public int idxInEndStateArray;
    public const int DefaultArraySize = 3;
    //public ConverterData(float _speed)
    //{
    //    srcIds = new ushort[DefaultArraySize];
    //    srcRequired = new byte[DefaultArraySize];
    //    srcCurrent = new byte[DefaultArraySize];
    //    targetCount = 1;
    //    targetId = -1;
    //    srcCurrent = null;

    //    timeToMakeOne = _speed;
    //    idxInUpdateArray = -1;
    //    idxInOutputStateArray = -1;
    //    pos = Vector3.zero;
    //}
    public void init( float _speed, int outputIdx) {
        srcIds = new ushort[DefaultArraySize];
        srcRequired = new byte[DefaultArraySize];
        srcCurrent = new byte[DefaultArraySize];

        timeToMakeOne = _speed;
        idxInUpdateArray = TubeSimulate.generic[1].addEntity();
        idxInEndStateArray = outputIdx;
    }
    public void setItemRequirements(ushort[] _srcIds, byte[] _srcReq, ushort _targetId, byte _targetCount) {
        for(int i = 0; i < _srcIds.Length; ++i) {
            srcIds[i] = (ushort)_srcIds[i];
            srcRequired[i] = (byte)_srcReq[i];
        }
        targetId = _targetId;
        targetCount = _targetCount;
    }

    void startUpdate() {
        GenericUpdateData d = TubeSimulate.generic[1].genericUpdateData[idxInUpdateArray];
        d.timeLeft = timeToMakeOne;
        TubeSimulate.generic[1].genericUpdateData[idxInUpdateArray] = d;
    }
    public bool hasSpace(int itemId) {
        for(int i = 0; i < DefaultArraySize; ++i) {
            if(srcIds[i] == itemId) {
                return srcCurrent[i] < srcRequired[i];
            }
        }
        return false;
    }
    public void push(ushort itemId) {
        Debug.Log("receives " + itemId);
        for (int i = 0; i < DefaultArraySize; ++i) {
            if (srcIds[i] == itemId) {
                srcCurrent[i]++;
            }
        }
        for (int i = 0; i < DefaultArraySize; ++i) {
            if (srcRequired[i] != srcCurrent[i]) {
                return;
            }
        }
        Debug.Log("converter starts!");
        //for (int i = 0; i < DefaultArraySize; ++i) {
        //    srcCurrent[i] = 0;
        //}
        clearCurrent();
        startUpdate();
    }

    public void clearCurrent() {
        for (int i = 0; i < DefaultArraySize; ++i) {
            srcCurrent[i] = 0;
        }
    }
    public void unblock() {
        GenericUpdateData d = TubeSimulate.generic[1].genericUpdateData[idxInUpdateArray];
        if (d.timeLeft >= 0.0f) return;

    }
    public void block() {

    }
    public void pop() {
        clearCurrent();
        startUpdate();
    }

    // visual related
    public Vector3 pos;
}
