using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ReplicatedProperties : MonoBehaviour {
    public bool alwaysRelevant = true;
    
    protected int goId;
    private void Awake() {
        goId = GetInstanceID();
    }
    // called on client
    public virtual void receive(int offset, int newVal) {

    }


    public virtual void receive(int offset, float newVal) {
    }

    public virtual void receive(int offset, int[] newIntArray) {
    }

    
}
