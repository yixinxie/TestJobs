using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public partial class CharacterMovement : ReplicatedProperties {
    CharacterController cc;
    public float height;
    public float speed = 3f;
    public float gravity = 9.8f;
    public Transform TPCameraTrans;
    Vector3 lastMousePos;
    protected override void Awake() {
        base.Awake();
        cc = GetComponent<CharacterController>();
        height = cc.height;
    }
    // Use this for initialization
    void Start () {
		
	}
    [RPC(isServer =1)]
    public void ReceiveUpdate(Vector3 pos, Vector3 rot) {
        transform.position = pos;
        transform.eulerAngles = rot;
    }
    public void LateUpdate() {
        //transform.po
        if (role == GameObjectRoles.Autonomous || role == GameObjectRoles.None) {
            //ClientTest.self.rpcBegin(goId, 16, SerializedBuffer.RPCMode_ToServer);
            //ClientTest.self.rpcAddParam(transform.position);
            //ClientTest.self.rpcAddParam(transform.eulerAngles);
            //ClientTest.self.rpcEnd();
            ReceiveUpdate_OnServer(transform.position, transform.eulerAngles);
        }
        else if (role == GameObjectRoles.Authority) {

        }
        else if (role == GameObjectRoles.SimulatedProxy) {

        }
    }
    public void updateMovement(float deltaTime) {
        // update movement.
        if (role == GameObjectRoles.Autonomous) {
            float verticalInput = Input.GetAxisRaw("Vertical");
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float jumpForce = 0.0f;
            if (cc.isGrounded == true) {
                if (Input.GetKeyDown(KeyCode.Space)) {
                    jumpForce = 100f;
                }
            }
            Vector3 moveInput = transform.forward * verticalInput + transform.right * horizontalInput + Vector3.up * jumpForce;
            moveInput.y -= gravity * deltaTime;
            //moveInput.Normalize();
            cc.Move(moveInput * deltaTime * speed);

            // jump
        }

    }
    public void updateLook(float deltaTime) {
        if (role == GameObjectRoles.Autonomous) {
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
            if (Mathf.Abs(diff.x) > 0f) {
                Vector3 angles = transform.eulerAngles;
                angles.y += diff.x;
                transform.eulerAngles = angles;
            }
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
