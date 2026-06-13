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
    [SerializeField] protected EnemyAnimatorController enemyAnimatorController;

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
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null) player = playerHealth.transform;
        }
        else
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
        if (enemyAnimatorController == null)
        {
            enemyAnimatorController = GetComponent<EnemyAnimatorController>();
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
        enemyAnimatorController?.UpdateMovement(CurrentState, agent);
    }

    protected abstract void UpdateState(bool canSeePlayer);
    protected abstract bool AcceptsInvestigation(InvestigationRecipient recipient);

    // プレイヤーを調査する
    public void Investigate(Vector3 position)
    {
        if (CurrentState == EnemyState.Chase || CurrentState == EnemyState.Attack ||
            CurrentState == EnemyState.AlertPrepare || CurrentState == EnemyState.Alarm)
        {
            return;
        }

        investigationPosition = position;
        SetState(EnemyState.Investigate); // 調査状態にする
        MoveTo(position); // 
    }

    protected void SetState(EnemyState nextState)
    {
        if (CurrentState == nextState) return;

        EnemyState previousState = CurrentState;
        CurrentState = nextState;
        stateTimer = 0f;

        enemyAnimatorController?.ApplyState(previousState, nextState);

        Debug.Log($"CurrentState:{CurrentState}");
    }

    protected void MoveTo(Vector3 position)
    {
        agent.isStopped = false;
        // NavMeshAgentに目的地を設定する関数
        // 敵をpositionの位置まで移動させる
        agent.SetDestination(position);
    }

    protected bool HasReachedDestination()
    {
        // pathPending:NavMeshAgentが現在、経路を計算中かどうか remainingDistance:NavMeshAgentの現在位置から目的地までの残り距離
        // 経路計算中ではないかつ目的地までの残り距離が十分小さいときtrue
        return !agent.pathPending &&
               agent.remainingDistance <= waypointReachDistance;
    }

    protected void UpdatePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (!agent.hasPath || HasReachedDestination())
        {
            MoveTo(patrolPoints[patrolIndex].position);
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length; // 次の巡回位置に更新
        }
    }

    // 視界にプレイヤーを見つけたか判定
    protected bool CanSeePlayer()
    {
        if (player == null || playerHealth == null || playerHealth.IsDown) return false;

        Vector3 origin = eyePoint.position; // 敵の視界の位置
        Vector3 target = player.position + Vector3.up; // プレイヤーの足元ではなく、少し上の位置を見る
        Vector3 toPlayer = target - origin; // プレイヤーへの方向

        if (toPlayer.magnitude > viewDistance) return false; // viewDistanceよりも距離が遠いなら見つけられない

        Vector3 flatDirection = Vector3.ProjectOnPlane(toPlayer, Vector3.up); // y軸方向をフラットにする
        float angle = Vector3.Angle(transform.forward, flatDirection); // 現在見ている方向とプレイヤー方向との角度を計算
        if (angle > viewAngle * 0.5f) return false;

        // Physics.Raycastは、指定した位置から指定した方向へ見えない線を飛ばし、Colliderに当たるか調べる処理
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

    // GameObjectを選択しているときだけ呼ばれるデバッグ描画用の関数
    protected virtual void OnDrawGizmosSelected()
    {
        Transform eye = eyePoint != null ? eyePoint : transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eye.position, viewDistance); // プレイヤーを見つけられる最大距離

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eye.position, Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * transform.forward * viewDistance);
        Gizmos.DrawRay(eye.position, Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward * viewDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.3f);
    }
}