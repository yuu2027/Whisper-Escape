using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerDebugDamage : MonoBehaviour
{
    [SerializeField] PlayerHealth health;
    [SerializeField] int damage = 1;

    void Awake()
    {
        if (health == null)
        {
            health = GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            health.TakeDamage(damage);
        }
    }
}