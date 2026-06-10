using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayHud : MonoBehaviour
{
    [SerializeField] PlayerHealth health;
    [SerializeField] PlayerInventory inventory;
    [SerializeField] TMP_Text hpText;
    [SerializeField] TMP_Text keyText;
    [SerializeField] TMP_Text controlsText;
    [SerializeField] TMP_Text downText;
    [SerializeField] TMP_Text clearText;
    [SerializeField] TMP_Text messageText;
    [SerializeField] float messageDuration = 2f;

    float messageTimer;

    void Start()
    {
        health.HpChanged += UpdateHp;
        health.DownChanged += SetDown;
        inventory.KeysChanged += UpdateKeys;

        UpdateHp(health.CurrentHp, health.MaxHp);
        UpdateKeys(inventory.KeyCount, inventory.RequiredKeys);

        controlsText.text = "WASD: move  \nMouse: perspective \nShift: run \nC: Sneak \nH: damage";
        downText.gameObject.SetActive(false);
        clearText.gameObject.SetActive(false);
        messageText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (messageTimer <= 0f) return;

        messageTimer -= Time.deltaTime;
        if (messageTimer <= 0f)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        health.HpChanged -= UpdateHp;
        health.DownChanged -= SetDown;
        inventory.KeysChanged -= UpdateKeys;
    }

    void UpdateHp(int current, int max)
    {
        hpText.text = $"HP: {current} / {max}";
    }

    void UpdateKeys(int current, int required)
    {
        keyText.text = $"Keys: {current} / {required}";
    }

    void SetDown(bool isDown)
    {
        downText.gameObject.SetActive(isDown);
        downText.text = "Down";
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        messageTimer = messageDuration;
    }

    public void ShowClear()
    {
        clearText.text = "Clear";
        clearText.gameObject.SetActive(true);
    }
}