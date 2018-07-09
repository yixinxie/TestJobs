using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConverterDebug : MonoBehaviour {
    int converterId;
    public float timeLeft;
    public int[] current;
    private void Awake() {
        current = new int[2];
    }
    // Use this for initialization
    void Start () {
        converterId = 0;
	}
	
	// Update is called once per frame
	void Update () {
        GenericUpdateData gud = TubeSimulate.generic[1].genericUpdateData[converterId];
        timeLeft = gud.timeLeft;
        ConverterData convDat = TubeSimulate.self.getConverterData(converterId);
        for (int i = 0; i < current.Length; ++i) {
            current[i] = convDat.srcCurrent[i];
        }

    }
}
