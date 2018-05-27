﻿//#define DEBUG_JOB
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using Unity.Jobs;
using Unity.Collections;

public enum SyncModes
{
    All,
    OwnerOnly,
    RemoteOnly,
}
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public class Replicated : Attribute
{
    int syncMode;

    public virtual SyncModes Mode
    {
        get { return (SyncModes)syncMode; }
        set { syncMode = (int)value; }
    }
}
public class ReflectionAttr : MonoBehaviour {
    [Replicated(Mode = SyncModes.OwnerOnly)]
    public int testInt;
    private void Awake()
    {

    }
    // Use this for initialization
    void Start () {
        const BindingFlags flags = /*BindingFlags.NonPublic | */BindingFlags.Public |
             BindingFlags.Instance | BindingFlags.Static;
        UnityEngine.Object obj = this;
        FieldInfo[] fields = obj.GetType().GetFields(flags);
        foreach (FieldInfo fieldInfo in fields)
        {
            //Attribute
            Attribute[] attributes = fieldInfo.GetCustomAttributes(typeof(Attribute), true) as Attribute[];
            if (attributes == null || attributes.Length == 0) continue;
            Replicated repAttr = attributes[0] as Replicated;
            Debug.Log(repAttr.Mode.ToString());
        }

    }
   
}
