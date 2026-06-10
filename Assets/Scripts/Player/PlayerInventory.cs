using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] int requiredKeys = 1;

    public int KeyCount { get; private set; }
    public int RequiredKeys => requiredKeys;

    public event Action<int, int> KeysChanged;

    public void AddKey()
    {
        KeyCount++;
        KeysChanged?.Invoke(KeyCount, requiredKeys);
    }

    public bool HasEnoughKeys()
    {
        return KeyCount >= requiredKeys;
    }
}