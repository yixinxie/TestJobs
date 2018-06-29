using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class FPSCharacter : ReplicatedProperties {
    CharacterMovement charMovement;
    public Transform MeshGO;
    public float speed = 2.5f;
    //public WeaponController currentWeapon;
    public Transform FPCameraTrans;
    public float gravity = 9.8f;
    Vector3 lastMousePos;
    CameraUtils cameraTrans;
    //BuildControl buildControl;
    bool isFirstPersonView;
    public bool startWith = false;
    int vehicleLayerMask;
    protected override void Awake() {
        base.Awake();
        charMovement = GetComponent<CharacterMovement>();
        vehicleLayerMask = LayerMask.GetMask(new string[] { "vehicle"});
    }
    // Use this for initialization
    protected override void initialReplicationComplete() {
        base.initialReplicationComplete();
        //if(currentWeapon != null)
        //    currentWeapon.setAudioSource(GetComponent<AudioSource>());

        //buildControl = Camera.main.GetComponent<BuildControl>();
        if(role == GameObjectRoles.Autonomous)
            possess();
    }
    public void possess() {
        cameraTrans = Camera.main.GetComponent<CameraUtils>();
        cameraTrans.lerpToAttach(charMovement.TPCameraTrans, 0.2f);
        Cursor.visible = false;
        //isFirstPersonView = false;
        //lastMousePos = Input.mousePosition;
        //enabled = true;
        //charMovement.enabled = true;
    }
    public void unpossess() {
        //enabled = false;
        //charMovement.enabled = false;
        //Cursor.visible = true;
    }
    
    public FPSCharacter drone;
    // Update is called once per frame
    void Update () {
        float deltaTime = Time.deltaTime;
        // update rotation
        charMovement.update(deltaTime);
        updateMouseClick();


        // build mode
        //if (Input.GetKeyDown(KeyCode.Escape)) {
        //    if(buildControl != null) {
        //        unpossess();
        //        buildControl.activate(this);
        //    }
        //}

        // drone
        if (charMovement.role == GameObjectRoles.Autonomous) {
            if (Input.GetKeyDown(KeyCode.T) && drone != null) {
                unpossess();

                drone.possess();
            }
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
    
    
}
