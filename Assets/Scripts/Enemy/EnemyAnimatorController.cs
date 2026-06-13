using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimatorController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] float vitualWalkSpeed = 2.0f;
    [SerializeField] float vitualAlertSpeed = 4.0f;
    [SerializeField] float speedDampTime = 0.12f;

    static readonly int Speed = Animator.StringToHash("Speed");
    static readonly int Attack = Animator.StringToHash("Attack");
    static readonly int Found = Animator.StringToHash("Found");
    static readonly int Lost = Animator.StringToHash("Lost");
    static readonly int Alarm = Animator.StringToHash("Alarm");

    bool waitingForFoundReaction;
    bool foundReactionStarted;
    bool waitingForLostReaction;
    bool lostReactionStarted;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    public void ApplyState(EnemyState previousState ,EnemyState nextState)
    {
        ResetOneShotTriggers();
        waitingForFoundReaction = nextState == EnemyState.AlertPrepare;
        waitingForLostReaction = nextState == EnemyState.LostPlayer;
        foundReactionStarted = false;
        lostReactionStarted = false;

        switch (nextState)
        {
            case EnemyState.Idle:
                animator.SetFloat(Speed, 0.0f);
                break;

            case EnemyState.Patrol:
                animator.SetFloat(Speed, vitualWalkSpeed);
                break;

            case EnemyState.AlertPrepare:
                animator.SetTrigger(Found);
                break;

            case EnemyState.Chase:
                animator.SetFloat(Speed, vitualAlertSpeed);
                break;

            case EnemyState.LostPlayer:
                animator.SetFloat(Speed, 0f);
                animator.SetTrigger(Lost);
                break;
        }
    }

    public void UpdateMovement(EnemyState state, NavMeshAgent agent)
    {
        if (animator == null) return;

        float speed = GetSpeedForState(state);

        if (speed == vitualWalkSpeed && !wantsToMove(agent))
        {
            speed = 0f;
        }

        animator.SetFloat(Speed, speed, speedDampTime, Time.deltaTime);
    }

    public bool IsFoundReactionFinished()
    {
        return IsReactionFinished(ref waitingForFoundReaction, ref foundReactionStarted, "Find_Player");
    }

    public bool IsLostReactionFinished()
    {
        return IsReactionFinished(ref waitingForLostReaction, ref lostReactionStarted, "Turn_Left", "Turn_Right");
    }

    float GetSpeedForState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Patrol:
            case EnemyState.Investigate:
            case EnemyState.SearchLastKnownPosition:
                return vitualWalkSpeed;   // Blend Tree: Walk = 2

            case EnemyState.Chase:
                return vitualAlertSpeed;  // Blend Tree: Chase = 4

            default:
                return 0f;                // Blend Tree: Idle = 0
        }
    }

    bool wantsToMove(NavMeshAgent agent)
    {
        if (agent == null || agent.isStopped) return false;
        if (agent.velocity.sqrMagnitude > 0.01f) return true;
        if (agent.pathPending) return true;

        return agent.hasPath && agent.remainingDistance > agent.stoppingDistance + 0.05f;
    }

    bool IsReactionFinished(ref bool waiting, ref bool started, params string[] stateNames)
    {
        if (!waiting || animator == null) return true;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        bool inReaction = false;
        foreach (string stateName in stateNames)
        {
            if (stateInfo.IsName(stateName))
            {
                inReaction = true;
                break;
            }
        }

        if (inReaction || animator.IsInTransition(0))
        {
            started = true;
        }

        bool finished = started && !animator.IsInTransition(0) && stateInfo.IsName("Locomotion");

        if (finished) waiting = false;
        return finished;
    }

    void ResetOneShotTriggers()
    {
        animator.ResetTrigger(Attack);
        animator.ResetTrigger(Found);
        animator.ResetTrigger(Lost);
        animator.ResetTrigger(Alarm);
    }

}
