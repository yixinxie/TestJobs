using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CharacterMovement : ReplicatedProperties {
    CharacterController cc;
    public float height;
    public float speed = 3f;
    public float gravity = 9.8f;
    [Replicated]
    public float serverTime;
    public Transform TPCameraTrans;
    Vector3 lastMousePos;
    float skinWidth;
    int movementCollisionLayer;
    protected override void Awake() {
        base.Awake();

        cc = GetComponent<CharacterController>();
        height = cc.height;
        skinWidth = cc.skinWidth;

        movementCollisionLayer = LayerMask.GetMask(new string[] { "Default", "Terrain" });
    }
    // Use this for initialization
    void Start() {
        if (ServerTest.self != null) {
            serverTime = Time.realtimeSinceStartup;
            rep_serverTime();
        }
    }
    float localTime;
    float timeDiff;
    [OnRep(forVar = "serverTime")]
    void onrepServerTime(float oldVal) {
        localTime = Time.realtimeSinceStartup;
        timeDiff = serverTime - localTime;
        Debug.Log("server time " + serverTime);
    }
    Vector3 lastKnownPos;
    Vector3 lastKnownRot;
    Vector3 lastKnownFrameVelocity;
    byte lastKnownInterpMode;
    float lastKnownTimestamp;
    [RPC(isServer = 1, Reliable = false)]
    public void ReceiveUpdate(Vector3 pos, Vector3 rot, float estTime, Vector3 _frameVelocity, byte interpolationMode) {
        //Debug.Log("rpc diff: " + (estTime - lastKnownTimestamp));

        lastKnownPos = pos;
        lastKnownRot = rot;
        lastKnownFrameVelocity = _frameVelocity;
        lastKnownInterpMode = interpolationMode;

        lastKnownTimestamp = estTime;
    }
    protected override void initialReplicationComplete() {
        base.initialReplicationComplete();
        Debug.Log("initial rep complete" + role);
        if (role != GameObjectRoles.Autonomous) {
            cc.enabled = false;
            Debug.Log("disabling cc on non autonomous characters.");
        }
    }
    public void onPossess() {
        lastMousePos = Input.mousePosition;
    }

    public Vector3 frameVelocity;
    Vector3 lerpedVelocity;
    public void update(float deltaTime) {
        Vector3 cachedPos = transform.position;
        if (role == GameObjectRoles.Autonomous) {
            Vector3 preUpdatePos = cachedPos;
            updateMovement(deltaTime);
            updateLook(deltaTime);
            frameVelocity = cachedPos - preUpdatePos;
            //frameVelocity /= deltaTime;
            frameVelocity.Normalize();
            frameVelocity *= speed;
            float sinceFirstRep = Time.realtimeSinceStartup + timeDiff;
            //if (frameVelocity.magnitude > 0.01f) {
            //    Debug.Log("frame velocity " + frameVelocity.magnitude);
            //}
            ReceiveUpdate_OnServer(transform.position, transform.eulerAngles, sinceFirstRep, frameVelocity, cc.isGrounded ? (byte)1 : (byte)0);
        }
        else if (role == GameObjectRoles.Authority) {
            float timePassedSinceLastKnown = Time.realtimeSinceStartup - lastKnownTimestamp;
            Vector3 predictedPos = Vector3.zero;
            if (lastKnownInterpMode == 1) { // linear interpolation
                predictedPos = timePassedSinceLastKnown * lastKnownFrameVelocity + lastKnownPos;
                cachedPos = (cachedPos + predictedPos) / 2f;
                //Debug.Log("time passed:" + timePassedSinceLastKnown + " last vel: " + lastKnownFrameVelocity + " last pos: " + lastKnownPos);

            }
            else {
                // lerp to.
                cachedPos = lastKnownPos;
            }
            transform.position = cachedPos;
            transform.eulerAngles = lastKnownRot;
        }
    }
    public bool grounded;
    Vector3 moveInputWhenJump;
    //Vector3 thisVelocity;
    void updateMovement(float deltaTime) {
        // update movement.
        Debug.DrawLine(transform.position, transform.position + Vector3.down * skinWidth * 2f, Color.green);
        cc.Move(velocity * deltaTime);
        bool jumpPressed = false;
        jumpPressed = Input.GetKey(KeyCode.Space);
        float verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        Vector3 moveInput = transform.forward * verticalInput + transform.right * horizontalInput;
        moveInput.Normalize();
        if (cc.isGrounded) {

            velocity = moveInput * speed;
            if (jumpPressed) {
                velocity += Vector3.up * 4.8f;
                moveInputWhenJump = moveInput;
            }
        }
        else {


            //velocity = moveInput * speed * 0.2f;
            velocity.y -= gravity * deltaTime;
            if(moveInputWhenJump.magnitude < 0.01f || Vector3.Dot(moveInput, moveInputWhenJump) < 0.0f) {
                velocity.x = moveInput.x * speed * 0.35f;
                velocity.z = moveInput.z * speed * 0.35f;
            }
            // what????
            cc.Move(velocity * deltaTime);
        }
        grounded = cc.isGrounded;


        // jump

    }
    Vector3 velocity;
    void updateLook(float deltaTime) {
        Vector3 mousepos = Input.mousePosition;
        Vector3 diff = mousepos - lastMousePos;
        float scrollDiff = Input.mouseScrollDelta.y;
        lastMousePos = mousepos;
        if (scrollDiff != 0f || Mathf.Abs(diff.y) > 0f) {
            Vector3 sphericalVec = TPCameraTrans.localPosition - Vector3.up * height;
            float radius = sphericalVec.magnitude;
            Vector2 polarCoords;
            toPolarCoords(sphericalVec, out polarCoords);
            // zoom in/out
            radius -= scrollDiff;
            // zoom clamping
            radius = Mathf.Clamp(radius, 0.5f, 80f);

            // vertical pan
            polarCoords.y += diff.y * deltaTime * 45f;

            // horizontal rotation
            polarCoords.y = Mathf.Clamp(polarCoords.y, 10f, 170f);
            Vector3 camPos;
            fromPolarCoords(polarCoords, radius, out camPos);
            TPCameraTrans.localPosition = camPos + Vector3.up * height;
            TPCameraTrans.LookAt(transform.position + Vector3.up * height);

            //if(currentWeapon != null)
            //    currentWeapon.transform.LookAt(currentWeapon.transform.position + TPCameraTrans.forward);
        }
        // self rotation
        if (Mathf.Abs(diff.x) > 0f) {
            Vector3 angles = transform.eulerAngles;
            angles.y += diff.x;
            transform.eulerAngles = angles;
        }
    }
    void toPolarCoords(Vector3 vec, out Vector2 polar) {
        float radius = vec.magnitude;
        polar.x = Mathf.Rad2Deg * Mathf.Atan2(vec.z, vec.x);
        polar.y = Mathf.Rad2Deg * Mathf.Acos(vec.y / radius);
    }
    void fromPolarCoords(Vector2 polar, float radius, out Vector3 cartesianCoords) {
        polar.x *= Mathf.Deg2Rad;
        polar.y *= Mathf.Deg2Rad;

        float radiusOnPlane = radius * Mathf.Sin(polar.y);
        cartesianCoords.x = radiusOnPlane * Mathf.Cos(polar.x);
        cartesianCoords.y = radius * Mathf.Cos(polar.y);
        cartesianCoords.z = radiusOnPlane * Mathf.Sin(polar.x);
    }
}

