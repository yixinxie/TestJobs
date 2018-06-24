using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUtils : MonoBehaviour {
    Transform targetTrans;
    float duration;
    float timeElapsed;

    // lerp to attach states.
    Vector3 fromPos;
    Quaternion fromRot;

    public Vector3 godviewEuler;
    public float godViewHeight = 50f;
    //BuildControl buildControl;
    int lerpTargetType;

    // godview states
    Vector3 targetPos;
    Vector3 targetEuler;
    Vector3 fromEuler;
    // Use this for initialization
    private void Awake() {
        //buildControl = GetComponent<BuildControl>();
    }
    void Start () {
		
	}

    public bool isInTransition() {
        return enabled;
    }

    public void lerpToGodView() {
        lerpTargetType = 2;
        targetEuler = godviewEuler;
        targetPos = transform.position;
        targetPos.y = godViewHeight;
        fromEuler = transform.eulerAngles;
        fromPos = transform.position;
        enabled = true;
        duration = 0.5f;
        timeElapsed = 0f;
    }
    public void lerpToAttach(Transform _targetTrans, float _duration) {
        if (targetTrans != null) return;
        if(_duration == 0f) {
            transform.SetParent(targetTrans, false);
            transform.localPosition = Vector3.zero;
            return;
        }
        targetTrans = _targetTrans;
        timeElapsed = 0f;
        duration = _duration;
        enabled = true;
        transform.SetParent(null, true);
        fromPos = transform.position;
        fromRot = transform.rotation;
        lerpTargetType = 1;
    }
	// Update is called once per frame
	void Update () {

        if (lerpTargetType == 1) {
            if (targetTrans != null) {
                timeElapsed += Time.deltaTime;
                float t = timeElapsed / duration;
                t = Mathf.Clamp(t, 0f, 1f);
                transform.position = Vector3.Lerp(fromPos, targetTrans.position, t);
                transform.rotation = Quaternion.Lerp(fromRot, targetTrans.rotation, t);
                if (t == 1f) {
                    transform.SetParent(targetTrans, true);
                    targetTrans = null;
                    enabled = false;
                }
            }
        }
        else if(lerpTargetType == 2) {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;
            t = Mathf.Clamp(t, 0f, 1f);
            transform.position = Vector3.Lerp(fromPos, targetPos, t);
            transform.eulerAngles = Vector3.Lerp(fromEuler, targetEuler, t);
            if (t == 1f) {
                transform.SetParent(null);
                enabled = false;
            }
        }
	}
}
