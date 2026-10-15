
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryPlayerInput : MonoBehaviour
{
    public static InventoryPickup NearbyPickup { get; private set; }

    private InventoryManager inventory;

    private void Start()
    {
        inventory = InventoryManager.Instance;

        if (inventory == null)
        {
            Debug.LogError("InventoryManager не найден!");
        }
    }

    private void Update()
    {
        if (inventory == null || Keyboard.current == null)
            return;

        // Открытие и закрытие инвентаря на Tab
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            inventory.ToggleInventory();
        }

        // Подбор предмета на E
        if (Keyboard.current.eKey.wasPressedThisFrame &&
            !inventory.IsOpen &&
            NearbyPickup != null)
        {
            NearbyPickup.PickUp();
        }
    }

    public static void SetNearbyPickup(InventoryPickup pickup)
    {
        NearbyPickup = pickup;
    }

    public static void ClearNearbyPickup(InventoryPickup pickup)
    {
        if (NearbyPickup == pickup)
        {
            NearbyPickup = null;
        }
    }
}
