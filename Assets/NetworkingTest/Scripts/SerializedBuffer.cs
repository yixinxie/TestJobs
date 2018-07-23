using System;
using System.Text;
using UnityEngine;

public class SerializedBuffer{
    byte[] src;
    int offset;
    /* rpc package process states */
    bool isInRPCSession;
    ushort commandCount;
    int rpcTotalLengthIndex;
    int capacity = 1024;
    public SerializedBuffer() {
        src = new byte[capacity];
        offset = 4;

    }
    public void serializeByte(byte byteVal) {
        src[offset] = byteVal;
        offset++;
    }
    public void serializeInt(int intVal) {
        byte[] intRaw = BitConverter.GetBytes(intVal);
        Array.Copy(intRaw, 0, src, offset, 4);
        offset += 4;
    }
    public void serializeUShort(ushort ushortVal) {
        byte[] ushortRaw = BitConverter.GetBytes(ushortVal);
        Array.Copy(ushortRaw, 0, src, offset, 2);
        offset += 2;
    }
    public void serializeFloat(float floatVal) {
        byte[] floatRaw = BitConverter.GetBytes(floatVal);
        Array.Copy(floatRaw, 0, src, offset, 4);
        offset += 4;
    }
    public void serializeString(string strVal) {
        // string length
        byte[] lengthBytes = BitConverter.GetBytes((ushort)strVal.Length);
        Array.Copy(lengthBytes, 0, src, offset, 2);
        offset += 2;

        // string content
        byte[] stringBytes = Encoding.ASCII.GetBytes(strVal);
        Array.Copy(stringBytes, 0, src, offset, stringBytes.Length);
        offset += stringBytes.Length;
    }
    public void incrementCommandCount() {
        commandCount++;
        doubleCapacityIfRequired();
    }
    void doubleCapacityIfRequired() {
        if (offset >= capacity * 0.75) {
            capacity *= 2;
            byte[] newArray = new byte[capacity];
            Array.Copy(src, 0, newArray, 0, offset);
            src = null;
            src = newArray;
            Debug.Log("serialized buffer capacity doubles:" + capacity);
        }
    }

    public void serializeVector3(Vector3 vec) {
        byte[] raw0 = BitConverter.GetBytes(vec.x);
        byte[] raw1 = BitConverter.GetBytes(vec.y);
        byte[] raw2 = BitConverter.GetBytes(vec.z);
        Array.Copy(raw0, 0, src, offset, 4);
        offset += 4;
        Array.Copy(raw1, 0, src, offset, 4);
        offset += 4;
        Array.Copy(raw2, 0, src, offset, 4);
        offset += 4;
    }
    public void serializeVector2(Vector2 vec) {
        byte[] raw0 = BitConverter.GetBytes(vec.x);
        byte[] raw1 = BitConverter.GetBytes(vec.y);
        Array.Copy(raw0, 0, src, offset, 4);
        offset += 4;
        Array.Copy(raw1, 0, src, offset, 4);
        offset += 4;
    }

    #region Replication
    public void repVar(int component_id, ushort offsetId, int intVal) {
        // netopcode, ushort
        serializeUShort((ushort)NetOpCodes.Replication);

        // total length, ushort
        serializeUShort(10);

        // component id, int
        serializeInt(component_id);

        // offset id, variable id, ushort
        serializeUShort(offsetId);

        // int value
        serializeInt(intVal);
        commandCount++;
        doubleCapacityIfRequired();
    }

    public void repVar(int component_id, ushort offsetId, byte byteVal) {
        // netopcode, ushort
        serializeUShort((ushort)NetOpCodes.Replication);

        // total length, ushort
        serializeUShort(7);

        // component id, int
        serializeInt(component_id);

        // offset id, variable id, ushort
        serializeUShort(offsetId);

        // byte value
        serializeByte(byteVal);
        commandCount++;
        doubleCapacityIfRequired();
    }

    public void repVar(int component_id, ushort offsetId, float floatVal) {
        // netopcode, ushort
        serializeUShort((ushort)NetOpCodes.Replication);

        // total length, ushort
        serializeUShort(10);

        // component id, int
        serializeInt(component_id);

        // offset id, variable id, ushort
        serializeUShort(offsetId);

        serializeFloat(floatVal);

        commandCount++;
        doubleCapacityIfRequired();
    }
    #endregion

    #region RPC
    public const byte RPCMode_None = 0;
    public const byte RPCMode_ToServer = 1; // 0001 not applicable on replicated variables
    public const byte RPCMode_ToTarget = 2; //  0010
    public const byte RPCMode_ExceptTarget = 4; //  0100
    public const byte RPCMode_ToAll = 6; //  0110 
    public const byte RPCMode_Unreliable = 8; //  1000 

    public void rpcBegin(int component_id, ushort rpc_id) {
        if(isInRPCSession) {
            Debug.LogError("the previous RPC did not finish!");
        }
        isInRPCSession = true;

        serializeUShort((ushort)NetOpCodes.RPCFunc);
        serializeInt(component_id);
        rpcTotalLengthIndex = offset;
        offset += 2;
        serializeUShort(rpc_id);
    }
    public void rpcEnd() {
        isInRPCSession = false;
        int bkOffset = offset;
        offset = rpcTotalLengthIndex;
        serializeUShort((ushort)bkOffset);
        offset = bkOffset;

        commandCount++;
        doubleCapacityIfRequired();
    }
    public void rpcAddParam(ushort ushortVal) {
        serializeUShort(ushortVal);
    }

    public void rpcAddParam(int intVal) {
        serializeInt(intVal);
    }

    public void rpcAddParam(byte byteVal) {
        serializeByte(byteVal);
    }

    public void rpcAddParam(float floatVal) {
        serializeFloat(floatVal);
    }

    public void rpcAddParam(Vector3 val) {
        serializeVector3(val);
    }

    public void rpcAddMovementTransform(Transform trans) {

    }
    #endregion
    public int getCommandCount() {
        return commandCount;
    }

    public int getOffset() {
        return offset;
    }

    public byte[] getBuffer() {
        return src;
    }

    public void seal() {
        int bkOffset = offset;
        offset = 0;
        serializeInt(commandCount);
        offset = bkOffset;
    }

    public void reset() {
        commandCount = 0;
        offset = 4;
    }
}