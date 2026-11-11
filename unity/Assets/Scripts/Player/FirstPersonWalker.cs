using UnityEngine;
using UnityEngine.InputSystem;

namespace PhoenicianJourney.Player
{
    /// <summary>
    /// First-person walking with mouse look, built on CharacterController so slopes, steps,
    /// and walls are handled by Unity. Values match the Babylon.js prototype.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonWalker : MonoBehaviour
    {
        [Header("Body (metres)")]
        [SerializeField] private float bodyHeight = 1.7f;
        [SerializeField] private float eyeHeight = 1.6f;
        [SerializeField] private float bodyRadius = 0.3f;
        [SerializeField] private float stepHeight = 0.35f;
        [SerializeField] private float maxWalkableSlopeDegrees = 40f;

        [Header("Movement (metres per second)")]
        [SerializeField] private float walkSpeed = 2.2f;
        [SerializeField] private float runSpeed = 5f;
        [SerializeField] private float gravity = 9.81f;

        [Header("Limits")]
        [Tooltip("The player can wade down to this ground height below the waterline (Y = 0).")]
        [SerializeField] private float minGroundY = -0.5f;
        [Tooltip("Falling below this height returns the player to the spawn point.")]
        [SerializeField] private float respawnBelowY = -10f;

        [Header("Look")]
        [SerializeField] private float lookSensitivity = 0.12f;

        private CharacterController controller;
        private Transform eye;
        private float pitch;
        private float verticalVelocity;
        private bool movementEnabled = true;
        private Vector3 spawnPosition;
        private float spawnYaw;

        public Camera Eye { get; private set; }

        public bool Grounded => controller != null && controller.isGrounded;

        public Vector3 Feet => transform.position;

        public float HorizontalSpeed { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            controller.height = bodyHeight;
            controller.radius = bodyRadius;
            controller.center = new Vector3(0f, bodyHeight / 2f, 0f);
            controller.stepOffset = stepHeight;
            controller.slopeLimit = maxWalkableSlopeDegrees;
            controller.skinWidth = 0.03f;

            Eye = GetComponentInChildren<Camera>();
            if (Eye == null)
            {
                var eyeObject = new GameObject("Eye", typeof(Camera), typeof(AudioListener));
                eyeObject.transform.SetParent(transform, false);
                Eye = eyeObject.GetComponent<Camera>();
            }

            eye = Eye.transform;
            eye.localPosition = new Vector3(0f, eyeHeight, 0f);
            Eye.nearClipPlane = 0.05f;
            Eye.fieldOfView = 63f;
            Eye.tag = "MainCamera";
        }

        private void Start()
        {
            SetSpawn(transform.position, transform.eulerAngles.y);
        }

        /// <summary>Sets where <see cref="Respawn"/> places the player, such as the export's SPAWN node.</summary>
        public void SetSpawn(Vector3 feet, float yawDegrees)
        {
            spawnPosition = feet;
            spawnYaw = yawDegrees;
        }

        public void Respawn()
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0f, spawnYaw, 0f));
            controller.enabled = true;
            pitch = 0f;
            eye.localRotation = Quaternion.identity;
            verticalVelocity = 0f;
        }

        /// <summary>
        /// Pauses walking and mouse look, for example while a 2D screen is open, and frees
        /// the cursor. Gravity keeps running so the player stays on the ground.
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;
            if (!enabled)
            {
                ReleaseCursor();
            }
        }

        private static void CaptureCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void ReleaseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (movementEnabled && mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
                {
                    CaptureCursor();
                }

                // Look while the cursor is captured, or while dragging with it free.
                if (Cursor.lockState == CursorLockMode.Locked || mouse.leftButton.isPressed)
                {
                    var delta = mouse.delta.ReadValue() * lookSensitivity;
                    transform.Rotate(0f, delta.x, 0f);
                    pitch = Mathf.Clamp(pitch - delta.y, -85f, 85f);
                    eye.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                }
            }

            var input = Vector2.zero;
            var running = false;
            if (movementEnabled && keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
                running = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    ReleaseCursor();
                }
            }

            Move(Vector2.ClampMagnitude(input, 1f), running, Time.deltaTime);
        }

        private void Move(Vector2 input, bool running, float deltaTime)
        {
            var start = transform.position;
            var speed = running ? runSpeed : walkSpeed;
            var horizontal = (transform.forward * input.y + transform.right * input.x) * speed;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                // A small downward push keeps the controller snapped to ramps and slopes.
                verticalVelocity = -2f;
            }
            verticalVelocity -= gravity * deltaTime;

            controller.Move((horizontal + Vector3.up * verticalVelocity) * deltaTime);

            // Stop at the wading limit instead of walking into deep water.
            if (transform.position.y < minGroundY && controller.isGrounded && start.y >= minGroundY)
            {
                controller.enabled = false;
                transform.position = start;
                controller.enabled = true;
            }

            var moved = transform.position - start;
            moved.y = 0f;
            HorizontalSpeed = deltaTime > 0f ? moved.magnitude / deltaTime : 0f;

            if (transform.position.y < respawnBelowY)
            {
                Respawn();
            }
        }
    }
}
