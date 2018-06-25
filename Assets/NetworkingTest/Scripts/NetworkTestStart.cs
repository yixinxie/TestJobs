using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NetworkTestStart : MonoBehaviour {
    // Use this for initialization
    void Start () {
        StreamReader sr = new StreamReader("network.config");
        Object obj;
        if (sr.ReadLine().Equals("server")) {
            obj = Resources.Load("Server");
        }
        else {
            obj = Resources.Load("Client");
        }
        GameObject.Instantiate(obj);
        sr.Close();
	}
}
