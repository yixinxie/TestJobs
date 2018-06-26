//#define DEBUG_JOB
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
    //string onRepFunc;

    public virtual SyncModes Mode
    {
        get { return (SyncModes)syncMode; }
        set { syncMode = (int)value; }
    }

    //public virtual string OnRep
    //{
    //    get { return onRepFunc; }
    //    set { onRepFunc = (string)value; }
    //}
}
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class OnRep : Attribute
{
    public string forVar;
}
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class RPC : Attribute
{
    bool reliable = true;
    byte syncMode;
    public byte isServer = 1;

    public virtual SyncModes Mode {
        get { return (SyncModes)syncMode; }
        set { syncMode = (byte)value; }
    }

    public virtual bool Reliable
    {
        get { return reliable; }
        set { reliable = (bool)value; }
    }
}
