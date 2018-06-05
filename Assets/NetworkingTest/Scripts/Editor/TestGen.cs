using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class TestGen {
    [MenuItem("Pathea Networking/Test Gen")]
    static void TestGene()
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | 
             BindingFlags.Instance | BindingFlags.Static;

        
        string generatedScriptPath = "NetworkingTest/Scripts";

        string repClassTemplate = @"
public partial class %name%{
    /** call this in Awake() */
    protected override void initNetworking()
    {
        base.initNetworking();
    }

    public override void receiveGeneric(ushort varOffset, byte[] src, ref int offset) {
        switch(varOffset){
            %switch_body%
        }
    }
}
";
        string repIntTmpl = @"
    public void rep_%var%() {
        ServerTest.self.rflcAddInt(goId, %offset%, %var%);
    }
";
        string repFloatTmpl = @"
    public void rep_%var%() {
        ServerTest.self.rflcAddFloat(goId, %offset%, %var%);
    }
";

        string IntSwitchCaseTmpl = @"
            case %var_offset%:
            {
                int previousVal = %var%;
                %var% = ClientTest.deserializeToInt(src, ref offset);
                %onrep%
            }
            break;
";
        string FloatSwitchCaseTmpl = @"
            case %var_offset%:
            {
                float previousVal = %var%;
                %var% = ClientTest.deserializeToFloat(src, ref offset);
                %onrep%
            }
            break;
";
        Type[] types = new Type[] {typeof(ReplicatedProperties_PlayerController) };

        for (int i = 0; i < types.Length; ++i) {
            Type thisType = types[i];
            FieldInfo[] fields = thisType.GetFields(flags);
            StreamWriter csOut = new StreamWriter("Assets/" + generatedScriptPath + "/" + thisType.ToString()+ "_generated.cs", false);
            int fieldIndex = 0;
            StringBuilder sendBody = new StringBuilder();
            StringBuilder switchBody = new StringBuilder();
            foreach (FieldInfo fieldInfo in fields)
            {
                //Attribute
                Attribute[] attributes = fieldInfo.GetCustomAttributes(typeof(Attribute), true) as Attribute[];
                FieldAttributes field_attributes = fieldInfo.Attributes;
                if (attributes == null || attributes.Length == 0) continue;
                Replicated repAttr = attributes[0] as Replicated;
                if (repAttr == null) continue;

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
                    insSwitch = insSwitch.Replace("%onrep%", "");
                    
                }
                else
                {
                    insSwitch = insSwitch.Replace("%onrep%", repAttr.OnRep + "(previousVal);");
                }
                switchBody.Append(insSwitch.ToString());

                fieldIndex++;
            }

            string fullClassText = repClassTemplate.Replace("%name%", thisType.ToString());
            fullClassText = fullClassText.Replace("%send_body%", sendBody.ToString());
            fullClassText = fullClassText.Replace("%switch_body%", switchBody.ToString());
            //fullClassText = fullClassText.Replace("%order%", "" + i);
            csOut.Write(fullClassText.ToString());
            csOut.Close();
        }
    }

}
