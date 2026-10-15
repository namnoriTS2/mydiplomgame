
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InventoryPickup : MonoBehaviour
{
    public InventoryItemData itemData;

    [Min(1)]
    public int amount = 1;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryPlayerInput.SetNearbyPickup(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryPlayerInput.ClearNearbyPickup(this);
        }
    }

    public void PickUp()
    {
        if (itemData == null)
        {
            Debug.LogWarning(
                "Не назначен Item Data у " + gameObject.name);
            return;
        }

        bool added = InventoryManager.Instance.TryAddItem(
            itemData, amount);

        if (added)
        {
            Debug.Log("Подобран предмет: " + itemData.itemName);
            InventoryPlayerInput.ClearNearbyPickup(this);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Недостаточно места в инвентаре!");
        }
    }
}
