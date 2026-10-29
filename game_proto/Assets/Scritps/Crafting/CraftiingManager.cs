
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [Header("Ресурсы")]
    public InventoryItemData gunpowderData;
    public InventoryItemData metalPartsData;
    public InventoryItemData clothData;
    public InventoryItemData chemicalsData;

    [Header("Результаты крафта")]
    public InventoryItemData pistolAmmoData;
    public InventoryItemData medkitData;

    [Header("Количество за создание")]
    public int pistolAmmoAmount = 12;
    public int medkitAmount = 1;

    private InventoryManager Inventory =>
        InventoryManager.Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int Count(InventoryItemData data)
    {
        if (Inventory == null || data == null)
            return 0;

        return Inventory.CountItemById(data.itemId);
    }

    public bool CanCraftAmmo()
    {
        return Inventory != null &&
               gunpowderData != null &&
               metalPartsData != null &&
               pistolAmmoData != null &&
               Inventory.HasItems(gunpowderData.itemId, 2) &&
               Inventory.HasItems(metalPartsData.itemId, 1);
    }

    public bool CanCraftMedkit()
    {
        return Inventory != null &&
               clothData != null &&
               chemicalsData != null &&
               medkitData != null &&
               Inventory.HasItems(clothData.itemId, 2) &&
               Inventory.HasItems(chemicalsData.itemId, 1);
    }

    public bool CraftAmmo()
    {
        if (!CanCraftAmmo())
        {
            Debug.Log("Недостаточно ресурсов для патронов.");
            return false;
        }

        // Сначала добавляем результат.
        // Если в инвентаре нет места, ресурсы не тратим.
        if (!Inventory.TryAddItem(pistolAmmoData, pistolAmmoAmount))
        {
            Debug.Log("Недостаточно места для патронов!");
            return false;
        }

        Inventory.ConsumeItemsById(gunpowderData.itemId, 2);
        Inventory.ConsumeItemsById(metalPartsData.itemId, 1);

        Inventory.RefreshUI();
        Inventory.UpdateAmmoUI();

        Debug.Log("Созданы патроны: " + pistolAmmoAmount);
        return true;
    }

    public bool CraftMedkit()
    {
        if (!CanCraftMedkit())
        {
            Debug.Log("Недостаточно ресурсов для аптечки.");
            return false;
        }

        if (!Inventory.TryAddItem(medkitData, medkitAmount))
        {
            Debug.Log("Недостаточно места для аптечки!");
            return false;
        }

        Inventory.ConsumeItemsById(clothData.itemId, 2);
        Inventory.ConsumeItemsById(chemicalsData.itemId, 1);

        Inventory.RefreshUI();

        Debug.Log("Аптечка создана!");
        return true;
    }
}
