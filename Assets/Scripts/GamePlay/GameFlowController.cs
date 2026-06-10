using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    [SerializeField] GameplayHud hud;

    public bool IsCleared { get; private set; }

    public void TryClear(PlayerInventory inventory)
    {
        if (IsCleared) return;

        if (!inventory.HasEnoughKeys())
        {
            hud.ShowMessage("ƒJƒM‚ª‘«‚è‚È‚¢");
            return;
        }

        IsCleared = true;
        hud.ShowClear();

        PlayerController controller = inventory.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetControlEnabled(false);
        }
    }
}