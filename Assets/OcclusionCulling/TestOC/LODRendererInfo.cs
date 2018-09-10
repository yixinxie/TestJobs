using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODRendererInfo : MonoBehaviour {
    void OnBecameVisible() {
        Debug.Log(name + " vis");
    }
    void OnBecameInvisible() {
        Debug.Log(name + " invis");
    }
}
