using UnityEngine;

public class AlertEnemyAI : EnemyBaseAI
{
    [SerializeField] float patrolSpeed = 1.6f;
    [SerializeField] float alertPrepareTime = 2f;
    [SerializeField] float alarmDuration = 3f;
    [SerializeField] float alarmRadius = 12f;
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
                if (canSeePlayer) SetState(EnemyState.AlertPrepare);
                else UpdatePatrol();
                break;

            case EnemyState.Investigate:
                agent.speed = patrolSpeed;
                if (canSeePlayer) SetState(EnemyState.AlertPrepare);
                else if (HasReachedDestination() && stateTimer >= investigationWaitTime) SetState(EnemyState.Patrol);
                break;

            case EnemyState.AlertPrepare:
                agent.isStopped = true;
                if (stateTimer >= alertPrepareTime)
                {
                    alarmSent = false;
                    SetState(EnemyState.Alarm);
                }
                break;

            case EnemyState.Alarm:
                agent.isStopped = true;

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
}