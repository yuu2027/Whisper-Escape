using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHp = 3;

    public int MaxHp => maxHp;
    public int CurrentHp { get; private set; }
    public bool IsDown { get; private set; }

    public event Action<int, int> HpChanged;
    public event Action<bool> DownChanged;

    void Awake()
    {
        CurrentHp = maxHp;
    }

    public void TakeDamage(int amount)
    {
        if (IsDown) return;

        CurrentHp = Mathf.Max(0, CurrentHp - amount);
        HpChanged?.Invoke(CurrentHp, maxHp);

        if (CurrentHp == 0)
        {
            IsDown = true;
            DownChanged?.Invoke(true);
        }
    }

    public void Recover()
    {
        if (!IsDown) return;

        IsDown = false;
        CurrentHp = maxHp;
        HpChanged?.Invoke(CurrentHp, maxHp);
        DownChanged?.Invoke(false);
    }
}