using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoveManager : MonoBehaviour {
    public static TestMoveManager self;
    List<TestMove> moves;
    // Use this for initialization
    public bool AddBunch;
    public bool RemoveBunch;
    public Object prototype;
    public float extent = 3f;
    public float startCount = 50;
    private void Awake() {
        self = this;
        moves = new List<TestMove>(128);
    }
    public void Add(TestMove move) {
        moves.Add(move);
    }
    public void Remove(TestMove move) {

        moves.Remove(move);
    }

    private void Start() {
        for (int i = 0; i < startCount; ++i) {
            GameObject.Instantiate(prototype, new Vector3(Random.Range(-extent, extent), 0f, Random.Range(-extent, extent)), Quaternion.identity);
        }
    }
    float elapsed;

    // Update is called once per frame
    void Update() {

        float dt = Time.deltaTime;
        elapsed += dt;
        for (int i = 0; i < moves.Count; ++i) {
            moves[i].update(dt);
        }

        if (AddBunch) {
            AddBunch = false;
            for (int i = 0; i < Random.Range(10, 15); ++i) {
                GameObject.Instantiate(prototype, new Vector3(Random.Range(-extent, extent), 0f, Random.Range(-extent, extent)), Quaternion.identity);
            }
        }
        if (RemoveBunch ) {
            RemoveBunch = false;
            int removeCount = Random.Range(10, 15);
            //removeCount = 1;
            for (int i = 0; i < removeCount; ++i) {
                if (moves.Count > 1) {
                    
                    int toRemove = Random.Range(1, moves.Count);
                    GameObject.Destroy(moves[toRemove].gameObject);
                    //moves.RemoveAt(toRemove);
                    
                }
            }
        }

        if (elapsed > 2f && elapsed < 100f) {
            //crash(0);
            //Debug.Assert(false);
            //throw new System.Exception();
            //throw new UnityException();
            //TestMove empty = null;
            //empty.outRange(null);
            //elapsed = 100f;

        }

    }
    private void crash(int x) {
        crash(x++);
    }
}
