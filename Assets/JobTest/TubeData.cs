using UnityEngine;

public struct TubeData : IPipe
{
    const int MaxElement = 16;
    public float speed;
    public float[] positions; // each value stores the distance to the next element in this array.
    public float length;
    public float itemHalfWidth;
    public short count, currentIndex;
    public ushort itemId;
    public int idxInUpdateArray; // index in tubeOutputUpdateData
    public int idxInEndStateArray; // index of the end state.
    public int idxInHeadEndStateArray; // index of the end state.

    // visual related
    public Vector3 from;
    public Vector3 toPos;

    public float getOffset()
    {
        TubeUpdateData d = TubeSimulate.self.tubeUpdateData[idxInUpdateArray];
        return d.current;
    }
    //public TubeData(float _length, float _speed)
    //{
    //    speed = _speed;
    //    length = _length;
    //    positions = new float[MaxElement];
    //    itemHalfWidth = 0.5f;
    //    count = 0;
    //    idxInUpdateArray = -1;
    //    currentIndex = -1;
    //    idxInOutputStateArray = -1;
    //    from = Vector3.zero;
    //    toPos = Vector3.zero;
    //}
    public void init(float _length, float _speed)
    {
        speed = _speed;
        length = _length;
        positions = new float[MaxElement];
        itemHalfWidth = 0.5f;
        count = 0;
        idxInUpdateArray = -1;
        idxInEndStateArray = -1;
        currentIndex = -1;
    }

    public bool hasSpace(ushort _itemId) {
        if (count == 0) return true;

        if(itemId != 0 && _itemId != itemId) {
            return false;
        }
        TubeUpdateData d = TubeSimulate.self.tubeUpdateData[idxInUpdateArray];
        return positions[0] + d.current - itemHalfWidth * 2f >= 0.0f;
    }
    public void push(ushort _itemId)
    {
        if(count >= MaxElement) return;
        itemId = _itemId;
        if(count > 0)
        {
            TubeUpdateData d = TubeSimulate.self.tubeUpdateData[idxInUpdateArray];

            if (positions[0] + d.current - itemHalfWidth * 2f >= 0.0f)
            {
                // refresh distances upto currentIndex.
                for (int i = 0; i <= currentIndex; ++i)
                {
                    positions[i] += d.current;
                }
                d.boundary -= d.current;
                d.current = 0.0f;
                TubeSimulate.self.tubeUpdateData[idxInUpdateArray] = d;

                for (int i = count; i > 0; --i)
                {
                    positions[i] = positions[i - 1];
                }
                positions[0] = 0.0f;
                count++;
                currentIndex++;
                if(currentIndex == 0)
                {
                    transfer();
                }
            }
        }
        else
        {
            positions[0] = 0.0f;
            currentIndex = 0;
            count++;
            transfer();
        }
    }
    // while previously in a blocked state, the last element just got unblocked.
    // this is called usually by the succeding node in the event that it just got unblocked.
    
    public void unblock()
    {
        // we also need to check if the tube is indeed in a blocked state.
        if (currentIndex >= count - 1) return;
        TubeUpdateData d = TubeSimulate.self.tubeUpdateData[idxInUpdateArray];
        for (int i = 0; i <= currentIndex; ++i)
        {
            positions[i] += d.current;
        }
        count--;
        currentIndex = (short)(count - 1);
        
        transfer();
    }
    // take a random element from the list. not usable yet!
    public void pick(float pickPos)
    {
        //float range = 0.25f;

    }
    // insert an element at random position. not usable yet!
    public void insert(float insertPos)
    {
        TubeUpdateData d = TubeSimulate.self.tubeUpdateData[idxInUpdateArray];
        float[] tmpPositions = new float[MaxElement];

        for (int i = 0; i < count; ++i)
        {
            if(i <= currentIndex)
            {
                tmpPositions[i] = positions[i] + d.current;
            }
            else
            {
                tmpPositions[i] = positions[i];
            }
        }
        tmpPositions = null;
    }
    
    private void transfer()
    {
        TubeUpdateData d = TubeSimulate.self.tubeUpdateData[idxInUpdateArray];
        d.current = 0.0f;
        if (currentIndex >= 0)
        { 
            if (currentIndex < count - 1)
            {
                d.boundary = positions[currentIndex + 1] - positions[currentIndex] - itemHalfWidth * 2f;
            }
            else
            {
                d.boundary = length - itemHalfWidth - positions[currentIndex];
            }
            d.speed = speed;
        }
        else
        {
            d.boundary = 0.0f;
            d.speed = 0.0f;
        }
        TubeSimulate.self.tubeUpdateData[idxInUpdateArray] = d;
    }
    // when an element can be removed.
    public void pop()
    {
        TubeUpdateData d = TubeSimulate.self.tubeUpdateData[idxInUpdateArray];
        for (int i = 0; i < currentIndex; ++i)
        {
            positions[i] += d.current;
        }
        currentIndex--;
        count--;
        transfer();
        
    }

    // when an element is blocked.
    public void block()
    {
        if (currentIndex >= 0)
        {
            TubeUpdateData d = TubeSimulate.self.tubeUpdateData[idxInUpdateArray];
            for (int i = 0; i <= currentIndex; ++i)
            {
                positions[i] += d.current;
            }
            positions[currentIndex] += d.boundary;
            currentIndex--;
            transfer();
        }
        
    }
}
