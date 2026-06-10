using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoalArea : MonoBehaviour
{
    [SerializeField] GameFlowController gameFlow;

    void Awake()
    {
        if (gameFlow == null)
        {
            gameFlow = FindAnyObjectByType<GameFlowController>();
        }

        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory == null) return;

        Debug.Log("ゴール！！！！！！！！！！！");

        gameFlow.TryClear(inventory);
    }
}