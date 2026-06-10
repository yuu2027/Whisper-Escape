using UnityEngine;

public class AttackEnemyAI : EnemyBaseAI
{
    [SerializeField] float patrolSpeed = 2.2f;
    [SerializeField] float chaseSpeed = 4.2f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float attackInterval = 1.2f;
    [SerializeField] int attackDamage = 1;

    float attackTimer;

    protected override void Awake()
    {
        base.Awake();
        SetState(EnemyState.Patrol);
    }

    protected override bool AcceptsInvestigation(InvestigationRecipient recipient)
    {
        return recipient == InvestigationRecipient.All ||
               recipient == InvestigationRecipient.AttackOnly;
    }

    protected override void UpdateState(bool canSeePlayer)
    {
        switch (CurrentState)
        {
            case EnemyState.Patrol:
                agent.speed = patrolSpeed;
                if (canSeePlayer) SetState(EnemyState.Chase);
                else UpdatePatrol();
                break;

            case EnemyState.Investigate:
                agent.speed = patrolSpeed;
                if (canSeePlayer) SetState(EnemyState.Chase);
                else if (HasReachedDestination() && stateTimer >= investigationWaitTime) SetState(EnemyState.Patrol);
                break;

            case EnemyState.Chase:
                agent.speed = chaseSpeed;
                if (canSeePlayer) MoveTo(player.position);

                if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange)
                    SetState(EnemyState.Attack);
                else if (lostTimer >= loseSightTime)
                    SetState(EnemyState.SearchLastKnownPosition);
                break;

            case EnemyState.Attack:
                agent.isStopped = true;
                attackTimer -= Time.deltaTime;

                if (!canSeePlayer || Vector3.Distance(transform.position, player.position) > attackRange)
                {
                    agent.isStopped = false;
                    SetState(canSeePlayer ? EnemyState.Chase : EnemyState.SearchLastKnownPosition);
                    return;
                }

                if (attackTimer <= 0f)
                {
                    playerHealth.TakeDamage(attackDamage);
                    attackTimer = attackInterval;
                }
                break;

            case EnemyState.SearchLastKnownPosition:
                agent.speed = patrolSpeed;
                MoveTo(lastKnownPlayerPosition);

                if (canSeePlayer) SetState(EnemyState.Chase);
                else if (HasReachedDestination() && stateTimer >= investigationWaitTime) SetState(EnemyState.Patrol);
                break;
        }
    }
}