using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public partial class CharacterMovement : ReplicatedProperties {
    CharacterController cc;
    public float height;
    public float speed = 3f;
    public float gravity = 9.8f;
    [Replicated]
    public float serverTime;
    [Replicated]
    public byte isHost;
    public Transform TPCameraTrans;
    Vector3 lastMousePos;
    float skinWidth;
    int movementCollisionLayer;

    float localTime;
    float timeDiff;
    Vector3 lastKnownPos;
    Vector3 lastKnownRot;
    Vector3 lastKnownFrameVelocity;
    byte lastKnownInterpMode;
    float lastKnownTimestamp;
    Vector3[] bezierPoints;

    StringBuilder debugSB;
    public bool grounded;
    Vector3 moveInputWhenJump;
    Vector3 velocity;

    List<PingStats> Pings;
    public float lastRTT;
    float avgRTT;
    public int remaining;
    
    protected override void Awake() {
        base.Awake();

        cc = GetComponent<CharacterController>();
        height = cc.height;
        skinWidth = cc.skinWidth;
        bezierPoints = new Vector3[4];
        Pings = new List<PingStats>();
        movementCollisionLayer = LayerMask.GetMask(new string[] { "Default", "Terrain" });
        //dbgFrames = new List<dbgFrame>();
    }
    // Use this for initialization
    void Start() {
        if (ServerTest.self != null) {
            serverTime = getTime();
            rep_serverTime();
        }
        bezierPoints[0] = transform.position;
        bezierPoints[1] = transform.position;
        bezierPoints[2] = transform.position;
        bezierPoints[3] = transform.position;
    }
    public static float getTime() {
        return Time.realtimeSinceStartup;
    }
    
    [OnRep(forVar = "serverTime")]
    void onrepServerTime(float oldVal) {
        localTime = getTime();
        timeDiff = serverTime - localTime;
        Debug.Log("time diff " + timeDiff);
    }

    [RPC(isServer = 1, Reliable = false)]
    public void ReceiveUpdate(Vector3 pos, Vector3 rot, float estTime, Vector3 _frameVelocity, byte interpolationMode) {
        //Debug.Log("rpc diff: " + (estTime - lastKnownTimestamp));
        //BezierStatic.evalBezier()
        lastKnownPos = pos;
        lastKnownRot = rot;
        lastKnownFrameVelocity = _frameVelocity;
        lastKnownInterpMode = interpolationMode;
        bezierPoints[0] = transform.position;
        bezierPoints[1] = bezierPoints[0];
        bezierPoints[3] = lastKnownPos;
        bezierPoints[2] = bezierPoints[3] + lastKnownFrameVelocity * 0.01f;
        //lastKnownTimestamp = estTime;
        lastKnownTimestamp = getTime();
        //Debug.Log("update " + lastKnownTimestamp);
        if(debugSB == null)
            debugSB = new StringBuilder(); 
    }
    void performRTTCalculation() {
        int id = Random.Range(0, 65535);
        Pings.Add(new PingStats(getTime(), id));
        PingServer_OnServer(id);
    }
    [RPC(isServer = 1, Reliable = false)]
    public void PingServer(int id) {
        PingClient_OnClient(id);
    }
    
    [RPC(isServer = 0, Reliable = false)]
    public void PingClient(int id) {
        for(int i = 0; i < Pings.Count; ++i) {
            if(Pings[i].id == id) {
                lastRTT = getTime() - Pings[i].time;
                Pings.Clear();
                break;
            }
        }
        
    }
    protected override void initialReplicationComplete() {
        base.initialReplicationComplete();
        Debug.Log("initial rep complete" + role);
        if (role != GameObjectRoles.Autonomous) {
            cc.enabled = false;
            Debug.Log("disabling cc on non autonomous characters.");
        }
        //if(ServerTest.self != null) {
        //    if(role == GameObjectRoles.Authority)
        //        controllerRole = GameObjectRoles.Autonomous;
        //}
        //else if (role == GameObjectRoles.Autonomous && ClientTest.self != null) {
        //    controllerRole = GameObjectRoles.Autonomous;
        //}
    }
    public void onPossess() {
        lastMousePos = Input.mousePosition;
    }
    List<dbgFrame> dbgFrames;
    struct dbgFrame {
        public float time;
        public Vector3 selfPos;
        public Vector3 selfVelocity;
        public Vector3 targetPos;
        public Vector3 targetVelocity;
        public float RTT;
    }
    public void update(float deltaTime) {
        
        if (role == GameObjectRoles.Autonomous) {
            remaining = Pings.Count;
            performRTTCalculation();
            Vector3 preUpdatePos = transform.position;
            updateMovement(deltaTime);
            updateLook(deltaTime);
            Vector3 frameVelocity = transform.position - preUpdatePos;
            //frameVelocity /= deltaTime;
            frameVelocity.Normalize();
            frameVelocity *= speed;
            float sinceFirstRep = getTime() + timeDiff;
            ReceiveUpdate_OnServer(transform.position, transform.eulerAngles, sinceFirstRep, frameVelocity, cc.isGrounded ? (byte)1 : (byte)0);
        }
        else if (role == GameObjectRoles.Authority) {
            Vector3 cachedPos = transform.position;
            float timePassedSinceLastKnown = getTime() - lastKnownTimestamp;
            Vector3 predictedPos = Vector3.zero;
            if (lastKnownInterpMode == 1 || true) { // linear interpolation
                //predictedPos = timePassedSinceLastKnown * lastKnownFrameVelocity + lastKnownPos;
                //cachedPos = predictedPos;
                //cachedPos = BezierStatic.evalBezier(bezierPoints[0], bezierPoints[1], bezierPoints[2], bezierPoints[3], timePassedSinceLastKnown * 10f);
                //Debug.LogFormat("vel {0}, lkf vel {1}, tslk {2}, dt {3}", velocity, lastKnownFrameVelocity, timePassedSinceLastKnown, deltaTime);
                Vector3 diffPos = lastKnownPos - cachedPos;
                if (diffPos.magnitude < speed * 1.5f * deltaTime) {
                    cachedPos = lastKnownPos;
                }
                else {
                    diffPos.Normalize();
                    diffPos *= speed * 1.5f;
                    velocity = Vector3.Lerp(velocity, diffPos, timePassedSinceLastKnown / 0.5f);
                    cachedPos += velocity * deltaTime;
                }
                //cachedPos = (cachedPos + predictedPos) / 2f;
                //Debug.Log("time passed:" + timePassedSinceLastKnown + " last vel: " + lastKnownFrameVelocity + " last pos: " + lastKnownPos);
                //Debug.Log("lerp " + timePassedSinceLastKnown + " lkt " + lastKnownTimestamp);
                //

            }
            else {
                // lerp to.
                cachedPos = lastKnownPos;
            }
            transform.position = cachedPos;
            transform.eulerAngles = lastKnownRot;
            if (dbgFrames != null) {
                dbgFrame frame = new dbgFrame();
                frame.time = getTime();
                frame.selfPos = cachedPos;
                frame.selfVelocity = velocity;
                frame.targetPos = lastKnownPos;
                frame.targetVelocity = lastKnownFrameVelocity;
                frame.RTT = lastRTT;
                dbgFrames.Add(frame);
            }
        }
        if (dumpDBGFrames) {
            dumpDBGFrames = false;
            StreamWriter sw = new StreamWriter("dbg_frames.txt", false);
            for(int i = 0; i < dbgFrames.Count; ++i) {
                sw.WriteLine("{0};{1},{2},{3};{4},{5},{6};{7},{8},{9};{10},{11},{12};{13}", 
                    dbgFrames[i].time, 
                    dbgFrames[i].selfPos.x, dbgFrames[i].selfPos.y, dbgFrames[i].selfPos.z,
                    dbgFrames[i].selfVelocity.x, dbgFrames[i].selfVelocity.y, dbgFrames[i].selfVelocity.z,
                    dbgFrames[i].targetPos.x, dbgFrames[i].targetPos.y, dbgFrames[i].targetPos.z,
                    dbgFrames[i].targetVelocity.x, dbgFrames[i].targetVelocity.y, dbgFrames[i].targetVelocity.z,
                    dbgFrames[i].RTT
                    );
                //sw.WriteLine(dbgFrames[i].time + ";" + dbgFrames[i].selfPos + ";" + dbgFrames[i].selfVelocity + ";"+ dbgFrames[i].targetPos + ";" + dbgFrames[i].targetVelocity + dbgFrames[i].RTT);
            }
            
            sw.Close();
        }
    }
    public bool dumpDBGFrames;

    
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

struct PingStats {
    public float time;
    public int id;
    public PingStats(float _time, int _id) {
        time = _time;
        id = _id;
    }
}