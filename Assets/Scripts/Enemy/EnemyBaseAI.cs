using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBaseAI : MonoBehaviour
{
    [SerializeField] protected Transform player;
    [SerializeField] protected Transform eyePoint;
    [SerializeField] protected Transform[] patrolPoints;
    [SerializeField] protected float viewDistance = 10f;
    [SerializeField] protected float viewAngle = 90f;
    [SerializeField] protected float loseSightTime = 3f;
    [SerializeField] protected float investigationWaitTime = 2f;
    [SerializeField] protected float waypointReachDistance = 0.4f;
    [SerializeField] protected LayerMask visionBlockMask = ~0;

    protected NavMeshAgent agent;
    protected PlayerHealth playerHealth;
    protected Vector3 lastKnownPlayerPosition;
    protected Vector3 investigationPosition;
    protected float stateTimer;
    protected float lostTimer;

    int patrolIndex;

    public EnemyState CurrentState { get; private set; }
    public Vector3 LastKnownPlayerPosition => lastKnownPlayerPosition;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (eyePoint == null) eyePoint = transform;

        if (player == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null) player = playerHealth.transform;
        }
        else
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    protected virtual void OnEnable()
    {
        EnemyInvestigationBroadcaster.InvestigationRequested += OnInvestigationRequested;
    }

    protected virtual void OnDisable()
    {
        EnemyInvestigationBroadcaster.InvestigationRequested -= OnInvestigationRequested;
    }

    protected virtual void Update()
    {
        stateTimer += Time.deltaTime;

        bool canSeePlayer = CanSeePlayer();
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.position;
            lostTimer = 0f;
        }
        else
        {
            lostTimer += Time.deltaTime;
        }

        UpdateState(canSeePlayer);
    }

    protected abstract void UpdateState(bool canSeePlayer);
    protected abstract bool AcceptsInvestigation(InvestigationRecipient recipient);

    public void Investigate(Vector3 position)
    {
        if (CurrentState == EnemyState.Chase || CurrentState == EnemyState.Attack ||
            CurrentState == EnemyState.AlertPrepare || CurrentState == EnemyState.Alarm)
        {
            return;
        }

        investigationPosition = position;
        SetState(EnemyState.Investigate);
        MoveTo(position);
    }

    protected void SetState(EnemyState nextState)
    {
        if (CurrentState == nextState) return;

        CurrentState = nextState;
        stateTimer = 0f;
    }

    protected void MoveTo(Vector3 position)
    {
        agent.isStopped = false;
        agent.SetDestination(position);
    }

    protected bool HasReachedDestination()
    {
        return !agent.pathPending &&
               agent.remainingDistance <= waypointReachDistance;
    }

    protected void UpdatePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (!agent.hasPath || HasReachedDestination())
        {
            MoveTo(patrolPoints[patrolIndex].position);
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }
    }

    protected bool CanSeePlayer()
    {
        if (player == null || playerHealth == null || playerHealth.IsDown) return false;

        Vector3 origin = eyePoint.position;
        Vector3 target = player.position + Vector3.up;
        Vector3 toPlayer = target - origin;

        if (toPlayer.magnitude > viewDistance) return false;

        Vector3 flatDirection = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
        float angle = Vector3.Angle(transform.forward, flatDirection);
        if (angle > viewAngle * 0.5f) return false;

        if (Physics.Raycast(origin, toPlayer.normalized, out RaycastHit hit, viewDistance, visionBlockMask))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }

        return false;
    }

    void OnInvestigationRequested(Vector3 position, float radius, InvestigationRecipient recipient)
    {
        if (!AcceptsInvestigation(recipient)) return;
        if (Vector3.Distance(transform.position, position) > radius) return;

        Investigate(position);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Transform eye = eyePoint != null ? eyePoint : transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eye.position, viewDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eye.position, Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * transform.forward * viewDistance);
        Gizmos.DrawRay(eye.position, Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward * viewDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.3f);
    }
}