﻿using System;
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
    static string rpc_tmpl = @"
    public void Server_%func%(%params%){
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
        int methodIndex = 0;
        for (int i = 0; i < methodInfo.Length; ++i)
        {
            RPC[] attributes = methodInfo[i].GetCustomAttributes(typeof(RPC), true) as RPC[];
            if (attributes == null || attributes.Length == 0) continue;
            StringBuilder ins = new StringBuilder(rpc_tmpl);
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
            for (int j = 0; j < paramInfo.Length; ++j)
            {
                ParameterInfo pInfo = paramInfo[j];
                if (pInfo.ParameterType.Equals(typeof(Int32)))
                {
                    serializeText.AppendLine("\t\tClientTest.self.rpcParamAddInt(goId, "+ methodIndex + ", " + pInfo.Name + ");");
                }
                else if (pInfo.ParameterType.Equals(typeof(Single)))
                {
                    serializeText.AppendLine("\t\tClientTest.self.rpcParamAddFloat(goId, " + methodIndex + ", " + pInfo.Name + ");");
                }
            }
            ins.Replace("%body%", serializeText.ToString());

            ret.AppendLine(ins.ToString());
            Debug.Log(ins);
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

            paramListSB.Append(paramInfo[j].Name);
            if (j < paramInfo.Length -1)
                paramListSB.Append(", ");

        }
        paramListText = paramListSB.ToString();
        return paramText.ToString();
    }

    static string csClassTmpl = @"
public partial class %name%{
    /** call this in Awake() */
    protected override void initNetworking()
    {
        base.initNetworking();
    }

    /** variable replication methods(server)*/
    %rep_body%

    /** variable reception method(client)*/
    public override bool stateRepReceive(ushort varOffset, byte[] src, ref int offset) {
        if(base.stateRepReceive(varOffset, src, ref offset)) return true;
        switch(varOffset){
            %rep_switch_body%
        }
        return true;
    }
    
    /** rpc serializers*/
    %rpc_body%

    /** rpc reception method(client)*/
    public override bool rpcReceive(ushort rpc_id, byte[] src, ref int offset) {
        if(base.rpcReceive(rpc_id, src, ref offset)) return true;
        switch(rpc_id){
            %rpc_switch_body%
        }
        return true;
    }
}
";  
    static string generateCSCode(Type thisType)
    {
        
        StringBuilder fullClassText = new StringBuilder(csClassTmpl.Replace("%name%", thisType.ToString()));

        string sendBody, switchBody;
        sendBody = generateStateRepCode(thisType, out switchBody);
        fullClassText.Replace("%rep_body%", sendBody);
        fullClassText.Replace("%rep_switch_body%", switchBody);

        string rpcSwitchCode;
        string rpcCode = generateRPCCode(thisType, out rpcSwitchCode);
        fullClassText.Replace("%rpc_body%", rpcCode);
        fullClassText.Replace("%rpc_switch_body%", rpcSwitchCode);
        return fullClassText.ToString();
    }

    static string repIntTmpl = @"
    public void rep_%var%() {
        ServerTest.self.rflcAddInt(goId, %offset%, %var%);
    }
";
    static string repFloatTmpl = @"
    public void rep_%var%() {
        ServerTest.self.rflcAddFloat(goId, %offset%, %var%);
    }
";

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
    static string generateStateRepCode(Type thisType, out string repSwitchCode)
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public |
             BindingFlags.Instance | BindingFlags.Static;
        FieldInfo[] fields = thisType.GetFields(flags);

        ushort fieldIndex = 64;
        StringBuilder sendBody = new StringBuilder();
        StringBuilder switchBody = new StringBuilder();
        foreach (FieldInfo fieldInfo in fields)
        {
            //Attribute
            Replicated[] attributes = fieldInfo.GetCustomAttributes(typeof(Replicated), true) as Replicated[];
            if (attributes == null || attributes.Length == 0) continue;
            Replicated repAttr = attributes[0];

            string ins = null;
            string insSwitch = null;
            if (fieldInfo.FieldType.Equals(typeof(int)))
            {
                ins = repIntTmpl;
                insSwitch = IntSwitchCaseTmpl;
            }
            else if (fieldInfo.FieldType.Equals(typeof(float)))
            {
                ins = repFloatTmpl;
                insSwitch = FloatSwitchCaseTmpl;
            }
            if (ins == null) continue;
            ins = ins.Replace("%var%", fieldInfo.Name);
            ins = ins.Replace("%offset%", "" + fieldIndex);
            sendBody.Append(ins);

            insSwitch = insSwitch.Replace("%var_offset%", "" + fieldIndex);
            insSwitch = insSwitch.Replace("%var%", fieldInfo.Name);
            if (string.IsNullOrEmpty(repAttr.OnRep))
            {
                insSwitch = insSwitch.Replace("%hasonrep%", "//");
                insSwitch = insSwitch.Replace("%onrep%", "");
            }
            else
            {
                insSwitch = insSwitch.Replace("%hasonrep%", "");
                insSwitch = insSwitch.Replace("%onrep%", repAttr.OnRep);
            }
            switchBody.Append(insSwitch.ToString());
            fieldIndex++;
        }
        repSwitchCode = switchBody.ToString();
        return sendBody.ToString();
    }
    
}
