using UnityEngine;

public class AlertEnemyAI : EnemyBaseAI
{
    [SerializeField] float patrolSpeed = 1.6f;
    [SerializeField] float alertPrepareTime = 2f;
    [SerializeField] float alarmDuration = 3f;
    [SerializeField] float alarmRadius = 12f;
    [SerializeField] float reactionFallbackTime = 3f;
    [SerializeField] AudioSource alarmAudio;

    bool alarmSent;

    protected override void Awake()
    {
        base.Awake();
        SetState(EnemyState.Patrol);
    }

    protected override bool AcceptsInvestigation(InvestigationRecipient recipient)
    {
        return recipient == InvestigationRecipient.All ||
               recipient == InvestigationRecipient.AlertOnly;
    }

    protected override void UpdateState(bool canSeePlayer)
    {
        switch (CurrentState)
        {
            case EnemyState.Patrol:
                agent.speed = patrolSpeed;
                agent.isStopped = false;

                if (canSeePlayer)
                {
                    StopAgent();
                    SetState(EnemyState.AlertPrepare);
                }
                else
                {
                    UpdatePatrol();
                }
                break;

            case EnemyState.Investigate:
                agent.speed = patrolSpeed;

                if (canSeePlayer)
                {
                    StopAgent();
                    SetState(EnemyState.AlertPrepare);
                }
                else if (HasReachedDestination() && stateTimer >= investigationWaitTime)
                {
                    SetState(EnemyState.Patrol);
                }
                break;

            case EnemyState.AlertPrepare:
                StopAgent();

                if (!canSeePlayer)
                {
                    SetState(EnemyState.LostPlayer);
                    break;
                }

                if (stateTimer >= alertPrepareTime &&
                    (enemyAnimatorController == null || enemyAnimatorController.IsFoundReactionFinished()))
                {
                    alarmSent = false;
                    agent.isStopped = false;
                    SetState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                agent.speed = patrolSpeed;

                if (canSeePlayer)
                {
                    agent.isStopped = false;
                    MoveTo(player.position);
                }
                else
                {
                    StopAgent();
                    SetState(EnemyState.LostPlayer);
                }
                break;

            case EnemyState.LostPlayer:
                StopAgent();

                if (enemyAnimatorController == null ||
                    enemyAnimatorController.IsLostReactionFinished() ||
                    stateTimer >= reactionFallbackTime)
                {
                    agent.isStopped = false;
                    SetState(EnemyState.Patrol);
                }
                break;

            case EnemyState.Alarm:
                StopAgent();

                if (!alarmSent)
                {
                    if (alarmAudio != null) alarmAudio.Play();

                    EnemyInvestigationBroadcaster.RequestInvestigation(
                        lastKnownPlayerPosition,
                        alarmRadius,
                        InvestigationRecipient.AttackOnly
                    );

                    alarmSent = true;
                }

                if (stateTimer >= alarmDuration)
                {
                    agent.isStopped = false;
                    SetState(EnemyState.Patrol);
                }
                break;
        }
    }

    void StopAgent()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

}