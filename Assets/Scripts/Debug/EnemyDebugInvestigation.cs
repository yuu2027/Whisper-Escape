using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyDebugInvestigation : MonoBehaviour
{
    [SerializeField] Camera targetCamera;
    [SerializeField] float radius = 20f;
    [SerializeField] InvestigationRecipient recipient = InvestigationRecipient.All;
    [SerializeField] LayerMask raycastMask = ~0;

    void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.jKey.wasPressedThisFrame) return;
        if (targetCamera == null) return;

        Ray ray = targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, raycastMask))
        {
            EnemyInvestigationBroadcaster.RequestInvestigation(hit.point, radius, recipient);
        }
    }
}