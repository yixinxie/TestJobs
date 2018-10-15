using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestArrayAccess : MonoBehaviour {
    public int count;
    float[] array;
    public bool reversed;
    public float outp;
    public Text textref;
    // Use this for initialization
    private void Awake() {
        array = new float[count];
    }
    int c = 0;
	// Update is called once per frame
	void Update () {
        float dt = Time.deltaTime;
        float sum = 0f;
        float begin = Time.realtimeSinceStartup;
        if (reversed) {
            for(int i = count - 1; i >= 0; --i) {
                array[i] += dt;
                sum += array[i];
            }
            //outp = sum;
        }
        else {
            for (int i = 0; i < count; ++i) {
                array[i] += dt;
                sum += array[i];
            }
            //outp = sum;
        }
        float end = Time.realtimeSinceStartup;
        c++;
        if ((c % 60) == 0) {
            textref.text = "time: " + (end - begin);
        }

    }
}
