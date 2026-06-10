using System;
using UnityEngine;

public static class EnemyInvestigationBroadcaster
{
    public static event Action<Vector3, float, InvestigationRecipient> InvestigationRequested;

    public static void RequestInvestigation(Vector3 position, float radius, InvestigationRecipient recipient)
    {
        InvestigationRequested?.Invoke(position, radius, recipient);
    }
}