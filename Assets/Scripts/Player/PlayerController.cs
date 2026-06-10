using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform cameraPivot;
    [SerializeField] float walkSpeed = 3.5f; // •à‚­‘¬‚³
    [SerializeField] float sprintSpeed = 6f; // ‘–‚Á‚½Žž‚Ì‘¬‚³
    [SerializeField] float crouchSpeed = 1.8f; // ‚µ‚á‚ª‚ÝŽž‚Ì‘¬‚³
    [SerializeField] float downSpeed = 0.6f; // ƒ_ƒEƒ“Žž‚Ì‘¬‚³
    [SerializeField] float lookSensitivity = 0.08f;
    [SerializeField] float standingHeight = 1.8f;
    [SerializeField] float crouchHeight = 1.1f;
    [SerializeField] float downHeight = 0.55f;
    [SerializeField] float gravity = -20f;

    CharacterController characterController;
    PlayerInput playerInput;
    PlayerHealth health;
    InputAction moveAction;
    InputAction lookAction;
    InputAction sprintAction;
    InputAction crouchAction;

    float pitch;
    float verticalVelocity;
    bool isCrouching;
    bool controlEnabled = true;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        health = GetComponent<PlayerHealth>();

        moveAction = playerInput.actions.FindAction("Move", true);
        lookAction = playerInput.actions.FindAction("Look", true);
        sprintAction = playerInput.actions.FindAction("Sprint", true);
        crouchAction = playerInput.actions.FindAction("Crouch", true);
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!controlEnabled) return;

        UpdateLook();
        UpdateCrouch();
        UpdateMovement();
        UpdateBodyHeight();
    }

    public void SetControlEnabled(bool enabled)
    {
        controlEnabled = enabled;
        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
    }

    void UpdateLook()
    {
        Vector2 look = lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector3.up * look.x * lookSensitivity);

        pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, -80f, 80f);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void UpdateCrouch()
    {
        if (health.IsDown) return;

        if (crouchAction.WasPressedThisFrame())
        {
            isCrouching = !isCrouching;
        }
    }

    void UpdateMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 direction = transform.right * input.x + transform.forward * input.y;
        direction = Vector3.ClampMagnitude(direction, 1f);

        float speed = GetCurrentSpeed();

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction * speed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }

    float GetCurrentSpeed()
    {
        if (health.IsDown) return downSpeed;
        if (isCrouching) return crouchSpeed;
        if (sprintAction.IsPressed()) return sprintSpeed;
        return walkSpeed;
    }

    void UpdateBodyHeight()
    {
        float targetHeight = health.IsDown ? downHeight : isCrouching ? crouchHeight : standingHeight;
        characterController.height = targetHeight;
        characterController.center = Vector3.up * (targetHeight * 0.5f);

        if (cameraPivot != null)
        {
            cameraPivot.localPosition = Vector3.up * (targetHeight - 0.15f);
        }
    }
}