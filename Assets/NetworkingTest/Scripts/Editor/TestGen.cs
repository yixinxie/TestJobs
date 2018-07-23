using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class TestGen {
    [MenuItem("Pathea Networking/Generate Pathea Networking Code")]
    static void GenerateNetworkingCode()
    {
        string generatedScriptPath = "NetworkingTest/Scripts";
        
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<Type> types = new List<Type>();
        for (int i = 0; i < assemblies.Length; ++i)
        {
            Type[] assemblyTypes = assemblies[i].GetTypes();
            for (int j = 0; j < assemblyTypes.Length; ++j)
            {
                if (assemblyTypes[j].IsSubclassOf(typeof(ReplicatedProperties)))
                {
                    types.Add(assemblyTypes[j]);
                }
            }
        }
        
        for (int i = 0; i < types.Count; ++i) {
            // one class.
            Type thisType = types[i];
            StreamWriter csOut = new StreamWriter("Assets/" + generatedScriptPath + "/" + thisType.ToString() + "_generated.cs", false);
            string fullClassText = generateCSCode(types[i]);
            csOut.Write(fullClassText);
            csOut.Close();
            Debug.Log("generated code for " + thisType.ToString());
        }
    }
    static string rpc_tmpServer = @"
    public void %func%_OnServer(%params%){
%body%
    }
";
    static string rpc_tmpClient = @"
    public void %func%_OnClient(%params%){
%body%
    }
";
    static string RpcSwitchCaseTmpl = @"
            case %rpc_id%:
            {
                %invoc%
            }
            break;
";
    static string generateRPCCode(Type thisType, out string switchText)
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public |
             BindingFlags.Instance | BindingFlags.Static;
        MethodInfo[] methodInfo = thisType.GetMethods(flags);
        StringBuilder ret = new StringBuilder();
        StringBuilder switchSB = new StringBuilder();
        int methodIndex = 64;
        for (int i = 0; i < methodInfo.Length; ++i)
        {
            RPC[] attributes = methodInfo[i].GetCustomAttributes(typeof(RPC), true) as RPC[];
            if (attributes == null || attributes.Length == 0) continue;
            string rpcAPI, rpcMode;
            StringBuilder ins;
            string hasOwnerParam = "";
            if (attributes[0].isServer == 1) {
                rpcAPI = "ClientTest.self";
                //rpcMode = "SerializedBuffer.RPCMode_ToServer";
                if (attributes[0].Reliable)
                    rpcMode = "";
                else
                    rpcMode = ", SerializedBuffer.RPCMode_Unreliable";
                ins = new StringBuilder(rpc_tmpServer);
                
            }
            else {
                rpcAPI = "ServerTest.self";
                if(attributes[0].Reliable)
                    rpcMode = ", SerializedBuffer.RPCMode_ToTarget | SerializedBuffer.RPCMode_ExceptTarget";
                else
                    rpcMode = ", SerializedBuffer.RPCMode_ToTarget | SerializedBuffer.RPCMode_ExceptTarget | SerializedBuffer.RPCMode_Unreliable";
                ins = new StringBuilder(rpc_tmpClient);
                hasOwnerParam = ", owner";
            }
             
            ins.Replace("%func%", methodInfo[i].Name);
            ParameterInfo[] paramInfo = methodInfo[i].GetParameters();
            StringBuilder paramText = new StringBuilder();
            for (int j = 0; j < paramInfo.Length; ++j)
            {
                paramText.Append(paramInfo[j].ParameterType.ToString() + " " + paramInfo[j].Name);
                if(j != paramInfo.Length - 1)
                    paramText.Append(",");
            }
            ins.Replace("%params%", paramText.ToString());
            StringBuilder serializeText = new StringBuilder();
            serializeText.AppendLine("\t\t" + rpcAPI + ".rpcBegin(goId, " + methodIndex + rpcMode + "" + hasOwnerParam + ");");
            for (int j = 0; j < paramInfo.Length; ++j)
            {
                ParameterInfo pInfo = paramInfo[j];
                //if (pInfo.ParameterType.Equals(typeof(Int32)) || pInfo.ParameterType.Equals(typeof(Single)))
                {
                    serializeText.AppendLine("\t\t" + rpcAPI + ".rpcAddParam(" + pInfo.Name + ");");
                }
            }
            serializeText.AppendLine("\t\t" + rpcAPI + ".rpcEnd();");
            ins.Replace("%body%", serializeText.ToString());

            ret.AppendLine(ins.ToString());
            //Debug.Log(ins);
            StringBuilder tmpsb = new StringBuilder(RpcSwitchCaseTmpl);
            string paramListText;
            string invokeVarDelc = generateRPCInvokeString(paramInfo, out paramListText);
            tmpsb.Replace("%rpc_id%", "" + methodIndex);
            string invocText = invokeVarDelc + methodInfo[i].Name + "(" + paramListText + ");";
            tmpsb.Replace("%invoc%", invocText);
            switchSB.AppendLine(tmpsb.ToString());
            // switch
            methodIndex++;

        }
        switchText = switchSB.ToString();
        return ret.ToString();
    }
    static Dictionary<string,string> generateOnrepCode(Type thisType) {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public |
             BindingFlags.Instance | BindingFlags.Static;
        MethodInfo[] methodInfo = thisType.GetMethods(flags);
        Dictionary<string, string> ret = new Dictionary<string, string>();
        for (int i = 0; i < methodInfo.Length; ++i) {
            OnRep[] attributes = methodInfo[i].GetCustomAttributes(typeof(OnRep), true) as OnRep[];
            if (attributes == null || attributes.Length == 0) continue;
            for (int j = 0; j < attributes.Length; ++j) {
                if(string.IsNullOrEmpty(attributes[j].forVar) == false) {
                    ret.Add(attributes[j].forVar, methodInfo[i].Name);
                }
            }
        }
        return ret;
    }
    static string generateRPCInvokeString(ParameterInfo[] paramInfo, out string paramListText)
    {
        StringBuilder paramText = new StringBuilder();
        StringBuilder paramListSB = new StringBuilder();
        for (int j = 0; j < paramInfo.Length; ++j)
        {
            ParameterInfo pInfo = paramInfo[j];
            if (pInfo.ParameterType.Equals(typeof(int)))
                paramText.Append(paramInfo[j].ParameterType.ToString() + " " + paramInfo[j].Name + " = ClientTest.deserializeToInt(src, ref offset);");
            else if (pInfo.ParameterType.Equals(typeof(float)))
                paramText.Append(paramInfo[j].ParameterType.ToString() + " " + paramInfo[j].Name + " = ClientTest.deserializeToFloat(src, ref offset);");
            else if (pInfo.ParameterType.Equals(typeof(Vector3)))
                paramText.Append(paramInfo[j].ParameterType.ToString() + " " + paramInfo[j].Name + " = ClientTest.deserializeToVector3(src, ref offset);");
            else if (pInfo.ParameterType.Equals(typeof(byte)))
                paramText.Append(paramInfo[j].ParameterType.ToString() + " " + paramInfo[j].Name + " = ClientTest.deserializeToByte(src, ref offset);");

            paramListSB.Append(paramInfo[j].Name);
            if (j < paramInfo.Length -1)
                paramListSB.Append(", ");

        }
        paramListText = paramListSB.ToString();
        return paramText.ToString();
    }

    static string csClassTmpl = @"
/** auto-generated file. do not modify unless you know what you are doing! */
public partial class %name%{

    /** variable replication methods(server)*/
    %rep_body%
    
    /** replicate all states upon gameobject replication*/
    public override void replicateAllStates(byte repMode, int conn_id = -1) {
        base.replicateAllStates(repMode, conn_id);
        %repvarlist%
    }

    /** variable reception method(client)*/
    %stateRep_rb%
    public override bool stateRepReceive(ushort varOffset, byte[] src, ref int offset) {
        if(base.stateRepReceive(varOffset, src, ref offset)) return true;
        switch(varOffset){
            %rep_switch_body%
        }
        return true;
    }
    %stateRep_re%
    
    /** rpc serializers*/
    %rpc_body%

    /** rpc reception method(client)*/
    %rpc_switch_rb%
    public override bool rpcReceive(ushort rpc_id, byte[] src, ref int offset) {
        if(base.rpcReceive(rpc_id, src, ref offset)) return true;
        switch(rpc_id){
            %rpc_switch_body%
        }
        return true;
    }
    %rpc_switch_re%
}
";  
    static string generateCSCode(Type thisType)
    {
        
        StringBuilder fullClassText = new StringBuilder(csClassTmpl.Replace("%name%", thisType.ToString()));

        string sendBody, switchBody, allStateRepList;
        Dictionary<string, string> onRepForVars = generateOnrepCode(thisType);
        sendBody = generateStateRepCode(thisType, out switchBody, out allStateRepList, onRepForVars);
        fullClassText.Replace("%rep_body%", sendBody);
        if (string.IsNullOrEmpty(switchBody) == false) {
            fullClassText.Replace("%rep_switch_body%", switchBody);
            fullClassText.Replace("%stateRep_rb%", "");
            fullClassText.Replace("%stateRep_re%", "");
        }
        else {
            fullClassText.Replace("%stateRep_rb%", "/*");
            fullClassText.Replace("%stateRep_re%", "*/");
        }
        fullClassText.Replace("%repvarlist%", allStateRepList);


        string rpcSwitchCode;
        string rpcCode = generateRPCCode(thisType, out rpcSwitchCode);
        fullClassText.Replace("%rpc_body%", rpcCode);
        if (string.IsNullOrEmpty(rpcSwitchCode) == false) {
            fullClassText.Replace("%rpc_switch_body%", rpcSwitchCode);
            fullClassText.Replace("%rpc_switch_rb%", "");
            fullClassText.Replace("%rpc_switch_re%", "");
        }
        else {
            fullClassText.Replace("%rpc_switch_rb%", "/*");
            fullClassText.Replace("%rpc_switch_re%", "*/");
        }
        return fullClassText.ToString();
    }

    static string repGenericTmpl = @"
    public void rep_%var%() {
        ServerTest.self.repVar(goId, %offset%, %var%, SerializedBuffer.RPCMode_ToTarget | SerializedBuffer.RPCMode_ExceptTarget);
    }
";
//    static string repFloatTmpl = @"
//    public void rep_%var%() {
//        ServerTest.self.repVar(goId, %offset%, %var%, SerializedBuffer.RPCMode_ToTarget | SerializedBuffer.RPCMode_ExceptTarget);
//    }
//";
//    static string repByteTmpl = @"
//    public void rep_%var%() {
//        ServerTest.self.repVar(goId, %offset%, %var%, SerializedBuffer.RPCMode_ToTarget | SerializedBuffer.RPCMode_ExceptTarget);
//    }
//";

    static string IntSwitchCaseTmpl = @"
            case %var_offset%:
            %hasonrep%int old_%var% = %var%;
            %var% = ClientTest.deserializeToInt(src, ref offset);
            %hasonrep%%onrep%(old_%var%);
            
            break;
";
    static string FloatSwitchCaseTmpl = @"
            case %var_offset%:
            %hasonrep%float old_%var% = %var%;
            %var% = ClientTest.deserializeToFloat(src, ref offset);
            %hasonrep%%onrep%(old_%var%);
            
            break;
";
    static string ByteSwitchCaseTmpl = @"
            case %var_offset%:
            %hasonrep%byte old_%var% = %var%;
            %var% = ClientTest.deserializeToByte(src, ref offset);
            %hasonrep%%onrep%(old_%var%);
            
            break;
";
    static string generateStateRepCode(Type thisType, out string repSwitchCode, out string allStateRepCode, Dictionary<string,string> onRepForVars)
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public |
             BindingFlags.Instance | BindingFlags.Static;
        FieldInfo[] fields = thisType.GetFields(flags);
        allStateRepCode = "";
        ushort fieldIndex = 64;
        StringBuilder sendBody = new StringBuilder();
        StringBuilder switchBody = new StringBuilder();
        foreach (FieldInfo fieldInfo in fields)
        {
            //Attribute
            Replicated[] attributes = fieldInfo.GetCustomAttributes(typeof(Replicated), true) as Replicated[];
            if (attributes == null || attributes.Length == 0) continue;

            string ins = null;
            string insSwitch = null;
            if (fieldInfo.FieldType.Equals(typeof(int)))
            {
                ins = repGenericTmpl;
                insSwitch = IntSwitchCaseTmpl;
            }
            else if (fieldInfo.FieldType.Equals(typeof(float)))
            {
                ins = repGenericTmpl;
                insSwitch = FloatSwitchCaseTmpl;
            }
            else if (fieldInfo.FieldType.Equals(typeof(byte))) {
                ins = repGenericTmpl;
                insSwitch = ByteSwitchCaseTmpl;
            }
            if (ins == null) continue;
            ins = ins.Replace("%var%", fieldInfo.Name);
            ins = ins.Replace("%offset%", "" + fieldIndex);
            sendBody.Append(ins);
            string allStateRepPart = "rep_%var%();".Replace("%var%", fieldInfo.Name);
            allStateRepCode += allStateRepPart;
            insSwitch = insSwitch.Replace("%var_offset%", "" + fieldIndex);
            insSwitch = insSwitch.Replace("%var%", fieldInfo.Name);
            if (onRepForVars.ContainsKey(fieldInfo.Name))
            {
                
                insSwitch = insSwitch.Replace("%hasonrep%", "");
                insSwitch = insSwitch.Replace("%onrep%", onRepForVars[fieldInfo.Name]);
            }
            else
            {
                insSwitch = insSwitch.Replace("%hasonrep%", "//");
                insSwitch = insSwitch.Replace("%onrep%", "");
            }
            switchBody.Append(insSwitch.ToString());
            fieldIndex++;
        }
        repSwitchCode = switchBody.ToString();
        return sendBody.ToString();
    }
    
}
