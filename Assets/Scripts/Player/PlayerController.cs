using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform cameraPivot;
    [SerializeField] float walkSpeed = 3.5f; // 歩く速さ
    [SerializeField] float sprintSpeed = 6f; // 走った時の速さ
    [SerializeField] float crouchSpeed = 1.8f; // しゃがみ時の速さ
    [SerializeField] float downSpeed = 0.6f; // ダウン時の速さ
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
    bool isCrouching; // しゃがんているか判定
    bool controlEnabled = true;

    public Vector2 MoveInput { get; private set; }
    public float InputMagnitude => Mathf.Clamp01(MoveInput.magnitude);
    public bool IsMoving => InputMagnitude > 0.05f;
    public bool IsCrouching => isCrouching;
    public bool IsGrounded => characterController != null && characterController.isGrounded;
    public bool IsSprinting => sprintAction != null && sprintAction.IsPressed() && !isCrouching && !health.IsDown;

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
        Cursor.lockState = CursorLockMode.Locked; // マウスカーソルを中央に固定する
        Cursor.visible = false; // マウスカーソルを非表示
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

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool wantsSprintFromCrouch = sprintAction.IsPressed() && moveInput.sqrMagnitude > 0.01f;

        if (isCrouching && wantsSprintFromCrouch)
        {
            isCrouching = false;
            return;
        }

        if (crouchAction.WasPressedThisFrame())
        {
            isCrouching = !isCrouching;
        }
    }

    void UpdateMovement()
    {
        MoveInput = moveAction.ReadValue<Vector2>();

        Vector3 direction = transform.right * MoveInput.x + transform.forward * MoveInput.y;
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
        if (sprintAction.IsPressed()) return sprintSpeed; // 走った時の速さ
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