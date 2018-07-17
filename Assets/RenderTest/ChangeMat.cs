using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMat : MonoBehaviour {
    public Material selfMat;
    public MeshRenderer selfRenderer;
    MaterialPropertyBlock mpb;
	// Use this for initialization
	void Start () {
        //selfMat.color = Color.red;
        //selfRenderer.sharedMaterial.color = Color.red;
        //selfRenderer.material.color = Color.red;
        mpb = new MaterialPropertyBlock();
        selfRenderer.GetPropertyBlock(mpb);
	}
    public bool setColor;
	// Update is called once per frame
	void Update () {
        if (setColor) {
            setColor = false;
            //selfRenderer.material.color = Color.red;
            mpb.SetColor("_Color", Color.red);
            selfRenderer.SetPropertyBlock(mpb);
        }
    }
}
