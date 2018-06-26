using System;
using System.Text;

public class SerializedBuffer{
    byte[] src;
    int offset;
    /* rpc package process states */
    byte rpcMode;
    ushort commandCount;
    int rpcTotalLengthIndex;
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

    #region Replication
    public void repInt(int component_id, ushort offsetId, int intVal) {
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
    }

    public void repFloat(int component_id, ushort offsetId, float floatVal) {
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
    }
    #endregion

    #region RPC
    public const byte RPCMode_None = 0;
    public const byte RPCMode_ToServer = 1; // 0001
    public const byte RPCMode_ToOwner = 2; //  0010
    public const byte RPCMode_ToRemote = 4; //  0100

    public void rpcBegin(int component_id, ushort rpc_id, byte _rpcMode) {
        rpcMode = _rpcMode;

        serializeUShort((ushort)NetOpCodes.RPCFunc);
        serializeInt(component_id);
        rpcTotalLengthIndex = offset;
        serializeUShort(rpc_id);
    }
    public void rpcEnd() {
        rpcMode = RPCMode_None;
        int bkOffset = offset;
        offset = rpcTotalLengthIndex;
        serializeUShort((ushort)bkOffset);
        offset = bkOffset;

        commandCount++;
    }
    public void rpcParamAddUShort(ushort ushortVal) {
        serializeUShort(ushortVal);
    }

    public void rpcParamAddInt(int intVal) {
        serializeInt(intVal);
    }

    public void rpcParamAddByte(byte byteVal) {
        serializeByte(byteVal);
    }

    public void rpcParamAddFloat(float floatVal) {
        serializeFloat(floatVal);
    }
    #endregion
    public int seal() {
        commandCount = 0;

        int length = offset;
        offset = 0;
        serializeInt(commandCount);
        return length;
    }
}