using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCharacter : MonoBehaviour {
    CharacterController charMovement;
    public Transform MeshGO;
    public float speed = 2.5f;
    //public WeaponController currentWeapon;
    public Transform FPCameraTrans;
    public Transform TPCameraTrans;
    public float gravity = 9.8f;
    Vector3 lastMousePos;
    CameraUtils cameraTrans;
    //BuildControl buildControl;
    bool isFirstPersonView;
    float headHeight;
    public bool startWith = false;
    int vehicleLayerMask;
    private void Awake() {
        charMovement = GetComponent<CharacterController>();
        vehicleLayerMask = LayerMask.GetMask(new string[] { "vehicle"});
    }
    // Use this for initialization
    void Start () {
        //if(currentWeapon != null)
        //    currentWeapon.setAudioSource(GetComponent<AudioSource>());

        //buildControl = Camera.main.GetComponent<BuildControl>();
        headHeight = charMovement.height;
        if(startWith)
            possess();
        else {
            enabled = false;
        }
    }
    public void possess() {
        cameraTrans = Camera.main.GetComponent<CameraUtils>();
        cameraTrans.lerpToAttach(TPCameraTrans, 0.2f);
        Cursor.visible = false;
        isFirstPersonView = false;
        lastMousePos = Input.mousePosition;
        enabled = true;
        charMovement.enabled = true;
    }
    public void unpossess() {
        enabled = false;
        charMovement.enabled = false;
        Cursor.visible = true;
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
    public FPSCharacter drone;
    // Update is called once per frame
    void Update () {
        float deltaTime = Time.deltaTime;
        // update rotation

        updateLook(deltaTime);
        updateMovement(deltaTime);
        updateMouseClick();


        // build mode
        //if (Input.GetKeyDown(KeyCode.Escape)) {
        //    if(buildControl != null) {
        //        unpossess();
        //        buildControl.activate(this);
        //    }
        //}

        // drone
        if (Input.GetKeyDown(KeyCode.T) && drone != null) {
            unpossess();
            
            drone.possess();
        }

        // interaction
        if (Input.GetKeyDown(KeyCode.F)) {
            //RaycastHit hit;
            //Vector3 interactRaycastTestOrigin = transform.position;

            //interactRaycastTestOrigin.y += headHeight / 2f;

            //if (Physics.Raycast(interactRaycastTestOrigin, transform.forward, out hit, 10f, vehicleLayerMask)) {
            //    Vehicle vehicle = hit.collider.GetComponent<Vehicle>();
            //    if(vehicle != null) {
            //        int seat = vehicle.getVacantSeat();
            //        if (seat >= 0) {
            //            unpossess();
            //            vehicle.mountPassenger(this.gameObject, seat);
            //        }

            //    }
            //}
        }
    }
    void updateMouseClick() {
        // weapon fire
        //bool leftClicked = Input.GetMouseButton(0);
        //if (leftClicked && currentWeapon != null) {
        //    currentWeapon.fireWeapon();
        //}

        //bool rightClicked = Input.GetMouseButton(1);
        //if (rightClicked && isFirstPersonView == false && cameraTrans.isInTransition() == false) {
        //    isFirstPersonView = true;
        //    UIDirectory.self.crosshairObj.SetActive(true);
        //    cameraTrans.lerpToAttach(FPCameraTrans, 0.2f);
        //}
        //else if (rightClicked == false && isFirstPersonView == true && cameraTrans.isInTransition() == false) {
        //    isFirstPersonView = false;
        //    UIDirectory.self.crosshairObj.SetActive(false);
        //    cameraTrans.lerpToAttach(TPCameraTrans, 0.2f);
        //}
    }
    void updateMovement(float deltaTime) {
        // update movement.
        float verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float jumpForce = 0.0f;
        if(charMovement.isGrounded == true) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                jumpForce = 100f;
            }
        }
        Vector3 moveInput = MeshGO.forward * verticalInput + MeshGO.right * horizontalInput + Vector3.up * jumpForce;
        moveInput.y -= gravity * deltaTime;
        //moveInput.Normalize();
        charMovement.Move(moveInput * deltaTime * speed);

        // jump
        
    }
    void updateLook(float deltaTime) {
        Vector3 mousepos = Input.mousePosition;
        Vector3 diff = mousepos - lastMousePos;
        float scrollDiff = Input.mouseScrollDelta.y;
        lastMousePos = mousepos;
        if (scrollDiff != 0f || Mathf.Abs(diff.y) > 0f) {
            Vector3 sphericalVec = TPCameraTrans.localPosition - Vector3.up * headHeight;
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
            TPCameraTrans.localPosition = camPos + Vector3.up * headHeight;
            TPCameraTrans.LookAt(transform.position + Vector3.up * headHeight);

            //if(currentWeapon != null)
            //    currentWeapon.transform.LookAt(currentWeapon.transform.position + TPCameraTrans.forward);
        }
        if (Mathf.Abs(diff.x) > 0f) {
            Vector3 angles = transform.eulerAngles;
            angles.y += diff.x;
            transform.eulerAngles = angles;
        }
    }
    
}
