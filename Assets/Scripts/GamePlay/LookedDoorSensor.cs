using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LockedDoorSensor : MonoBehaviour
{
    [SerializeField] LockedDoor lockedDoor;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (lockedDoor == null)
        {
            lockedDoor = GetComponentInParent<LockedDoor>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory == null) return;

        lockedDoor.TryOpen(inventory);
    }
}