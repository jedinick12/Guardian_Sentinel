using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 1f;
    public float maxDistance = 5f;
    public float collisionPadding = 0.1f;
    [Range(0.01f, 1f)]
    public float returnSpeed = 0.1f;

    private float mouseX, mouseY;

    public Transform target, player;

    private void Start() {
        // Lock and hide the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Set mouse rotation to initial camera rotation
        Vector3 playerRot = player.eulerAngles;
        mouseX = playerRot.y;
    }

    private void LateUpdate() {
        CamControl();

        CollisionCheck();
    }

    private void CamControl() {
        // Get mouse input and clamp it
        mouseX += Input.GetAxis("Mouse X") * rotationSpeed;
        mouseY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        mouseY = Mathf.Clamp(mouseY, -35, 60);

        transform.LookAt(target);

        target.rotation = Quaternion.Euler(mouseY, mouseX, 0);
    }

    private void CollisionCheck() {
        Vector3 newLocalPos = transform.localPosition;

        // Get direction from target to camera
        Vector3 direction = transform.position - target.position;
        direction.Normalize();

        // Check if something is in between the player and the camera
        RaycastHit hit;
        if (Physics.Raycast(target.position, direction, out hit, maxDistance)) {
            if (hit.transform.tag != "Player" && hit.collider.isTrigger == false) {
                if (hit.distance < Mathf.Abs(transform.localPosition.z)) {
                    // If camera is behind the hit point, move cam forward
                    newLocalPos.z = -(hit.distance - collisionPadding);
                } else {
                    // If camera is infront of hit point, lerp cam backwards
                    newLocalPos.z = Mathf.Lerp(newLocalPos.z, -(hit.distance - collisionPadding), returnSpeed);
                }
            }
        } else {
            // If camera is not at max distance, lerp backwards
            newLocalPos.z = Mathf.Lerp(newLocalPos.z, -maxDistance, returnSpeed);
        }

        // Update the camera's local position
        transform.localPosition = newLocalPos;
    }

    public float GetMouseX() {
        return mouseX;
    }
}
