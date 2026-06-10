using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [SerializeField] Collider blockingCollider;
    [SerializeField] GameObject doorVisual;
    [SerializeField] GameplayHud hud;

    bool isOpen;

    public void TryOpen(PlayerInventory inventory)
    {
        if (isOpen) return;

        if (!inventory.HasEnoughKeys())
        {
            hud.ShowMessage("Missing Keys");
            return;
        }

        isOpen = true;

        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
        }

        if (doorVisual != null)
        {
            doorVisual.SetActive(false);
        }
    }
}