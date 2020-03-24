using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
        public float speed = 5f;
        public float gravity = -9f;
        public float gravFallMultiplier = 1f;
        public float gravJumpMultiplier = 0.5f;
        public float jumpHeight = 1f;
        [Range(0, 1)]
        public float airControlPercent = 0.8f;
        public float turnSmoothTime = 0.05f;
        public float speedSmoothTime = 0.05f;
        public float animDamping = 0.1f;
        public bool doubleJumpEnabled;

        private float velocityY;
        private float turnSmoothVelocity;
        private float currentSpeed;
        private float speedSmoothVelocity;
        private Vector3 additionalRot;
        private bool canDoubleJump;
        private float lookForwardTime;

        public CameraController camController;
        //public Animator animator;

        private CharacterController controller;

        private void Awake() {
            controller = GetComponent<CharacterController>();
            // navMeshAgent.updateRotation = false;
            if (!camController) {
                Debug.LogError("Player controller requires reference to camera controller.");
            }
        }

        private void Update() {
            // Get movement input
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

            // Align rotation with camera
            AlignRotationWithCam(input);

            // Reset double jump if grounded
            if (controller.isGrounded) {
                canDoubleJump = true;

                // Update animator
                //animator.SetBool("isGrounded", true);
            } else {
                // Update animator
                //animator.SetBool("isGrounded", false);
            }

            // Handle jumping
            if (Input.GetButtonDown("Jump")) {
                if (controller.isGrounded) {
                    Jump();
                } else if (doubleJumpEnabled && canDoubleJump) {
                    Jump();
                    canDoubleJump = false;
                }
            }

            // Move
            Move(input);

            // Animate
            //animator.SetFloat("speedPercent", currentSpeed / speed, animDamping, Time.deltaTime);
        }

        private void Move(Vector3 input) {
            // Rotate player
            Vector3 inputDir = input.normalized;
            if (inputDir != Vector3.zero) {
                // Add rotation according to move direction (this additional rotation is smoothed)
                float targetAdditionalRot;
                if (lookForwardTime > 0) {
                    // Player must look forward (e.g. if attacking)
                    targetAdditionalRot = Mathf.Atan2(0f, 1f) * Mathf.Rad2Deg;
                    lookForwardTime -= 1f * Time.deltaTime;
                } else {
                    // Not forced to look forward, rotate based on input
                    targetAdditionalRot = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
                }
                additionalRot = Vector3.up * Mathf.SmoothDampAngle(additionalRot.y, targetAdditionalRot, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
                transform.eulerAngles += additionalRot;
            }

            // Gravity
            ApplyGravity();

            // Smooth speed
            float targetSpeed = speed * input.normalized.magnitude;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

            // Move player
            Vector3 moveDir = Quaternion.AngleAxis(camController.GetMouseX(), Vector3.up) * input.normalized;
            moveDir.Normalize();
            Vector3 move = moveDir * currentSpeed;
            move += Vector3.up * velocityY;
            controller.Move(move * Time.deltaTime);

            currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

            // If grounded, reset Y velocity
            if (controller.isGrounded) {
                velocityY = 0f;
            }
        }

        public void LookForward(float time) {
            lookForwardTime = time;
        }

        public void SetPosition(Vector3 pos) {
            transform.position = pos;
        }

        public void SetYVelocity(float vel) {
            velocityY = vel;
        }

        private void Jump() {
            // Calculate velocity based on jump height
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -2 * gravity);
            velocityY = jumpVelocity;

            // Animate
            //animator.SetTrigger("jump");
        }

        private void ApplyGravity() {
            float grav = gravity;

            // Apply different gravity modifiers if player is falling or jumping
            if (controller.velocity.y < 0) {
                grav *= gravFallMultiplier;
            } else if (controller.velocity.y > 0) {
                grav *= gravJumpMultiplier;
            }

            velocityY += grav * Time.deltaTime;
        }

        private float GetModifiedSmoothTime(float smoothTime) {
            if (controller.isGrounded) {
                return smoothTime;
            }
            if (airControlPercent == 0) {
                return float.MaxValue;
            }

            return smoothTime / airControlPercent;
        }

        private void AlignRotationWithCam(Vector3 move) {
            // If the player is moving, align the player rotation with the camera (if the reference exists)
            if (camController && move.magnitude > 0) {
                transform.rotation = Quaternion.Euler(0, camController.GetMouseX(), 0);
            }
        }
}
