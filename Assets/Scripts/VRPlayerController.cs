using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class VRPlayerController : MonoBehaviour
{
    [Header("Hareket (XR Input)")]
    public InputActionReference moveAction;
    public InputActionReference turnAction;
    public float walkSpeed  = 5f;
    public float gravity    = -20f;
    public float jumpHeight = 1.2f;

    [Header("Kamera / Bakis")]
    public Transform cameraTransform;
    public float turnSpeed = 100f; // Turn speed for joystick
    public float mouseSensitivity = 2f; // For desktop mouse fallback

    [Header("Saglik")]
    public float maxHealth = 200f;

    private CharacterController cc;
    private Vector3 velocity;
    private float currentHealth;
    private bool isDead;
    private float cameraPitch = 0f;

    void Awake()
    {
        cc            = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        
        // Lock cursor for easy FPS testing in editor
        #if UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        #endif
    }

    void OnEnable()
    {
        if (moveAction != null && moveAction.action != null) moveAction.action.Enable();
        if (turnAction != null && turnAction.action != null) turnAction.action.Enable();
    }

    void OnDisable()
    {
        if (moveAction != null && moveAction.action != null) moveAction.action.Disable();
        if (turnAction != null && turnAction.action != null) turnAction.action.Disable();
    }

    void Update()
    {
        if (isDead) return;

        MovePlayer();
        RotateCamera();
        
        // Unlock cursor logic just in case
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void MovePlayer()
    {
        Vector2 input = Vector2.zero;
        if (moveAction != null && moveAction.action != null)
        {
            input = moveAction.action.ReadValue<Vector2>();
        }

        // Fallback for Editor (WASD)
        if (input.sqrMagnitude < 0.01f)
        {
            input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        // Move relative to camera look direction to feel like an FPS, or body if preferred.
        // We use cameraTransform to figure out forward direction if we want true head-oriented movement.
        Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;
        
        forward.y = 0; right.y = 0; // Keep movement flat
        forward.Normalize(); right.Normalize();

        Vector3 direction = right * input.x + forward * input.y;
        cc.SimpleMove(direction * walkSpeed);

        if (!cc.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            cc.Move(velocity * Time.deltaTime);
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void RotateCamera()
    {
        Vector2 turnInput = Vector2.zero;
        if (turnAction != null && turnAction.action != null)
        {
            turnInput = turnAction.action.ReadValue<Vector2>();
        }

        // XR Turn applies to the body (Y axis)
        if (Mathf.Abs(turnInput.x) > 0.1f)
        {
            transform.Rotate(0, turnInput.x * turnSpeed * Time.deltaTime, 0);
        }

        // Fallback for Desktop Mouse Look
        if (turnInput.sqrMagnitude < 0.01f && Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotate body horizontally
            transform.Rotate(0, mouseX, 0);

            // Rotate camera vertically
            if (cameraTransform != null)
            {
                cameraPitch -= mouseY;
                cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
                cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;
        currentHealth -= dmg;
        if (currentHealth <= 0f)
        {
            isDead = true;
            Debug.Log("[VRPlayer] Oyuncu oldu!");
        }
    }
}
