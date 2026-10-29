
using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingResourcePickup : MonoBehaviour
{
    [Header("Предмет")]
    public InventoryItemData itemData;
    public int amount = 1;

    private bool playerNearby;

    private void Update()
    {
        if (!playerNearby || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("InventoryManager не найден!");
                return;
            }

            if (itemData == null)
            {
                Debug.LogError(
                    "Не назначен Item Data у " + gameObject.name);
                return;
            }

            bool added = InventoryManager.Instance.TryAddItem(
                itemData, amount);

            if (added)
            {
                Debug.Log("Подобран предмет: " + itemData.itemName);
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Недостаточно места в инвентаре!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Нажмите E, чтобы подобрать: " + gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}
