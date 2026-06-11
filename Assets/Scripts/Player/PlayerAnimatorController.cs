using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] PlayerController controller;
    [SerializeField] PlayerHealth health;

    static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    static readonly int Crouch = Animator.StringToHash("Crouch");
    static readonly int Grounded = Animator.StringToHash("Grounded");
    static readonly int Die = Animator.StringToHash("Die");
    static readonly int Recover = Animator.StringToHash("Recover");

    private bool wasDown; // すでに死亡状態か確認

    void Awake()
    {
        if (controller == null) controller = GetComponentInParent<PlayerController>();
        if (health == null) health = GetComponentInParent<PlayerHealth>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (health != null)
        {
            wasDown = health.IsDown;
        }
    }

    void Update()
    {
        if (animator == null || controller == null || health == null) return;

        bool isDown = health.IsDown;

        if (isDown && !wasDown)
        {
            animator.ResetTrigger(Recover);
            animator.SetTrigger(Die);
        }
        else if (!isDown && wasDown)
        {
            animator.ResetTrigger(Die);
            animator.SetTrigger(Recover);
        }

        float moveSpeed = 0f;

        if (controller.IsMoving)
        {
            if (health.IsDown || controller.IsCrouching)
            {
                moveSpeed = 1f;
            }
            else
            {
                // しゃがんでたら1、しゃがんでいないなら0.5
                moveSpeed = controller.IsSprinting ? 1f : 0.5f;
            }
        }

        animator.SetFloat(MoveSpeed, moveSpeed, 0.12f, Time.deltaTime);
        animator.SetBool(Crouch, controller.IsCrouching && !isDown);
        animator.SetBool(Grounded, controller.IsGrounded);

        wasDown = isDown;
    }
}