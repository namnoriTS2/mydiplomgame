
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class InventoryPickup : MonoBehaviour
{
    public InventoryItemData itemData;
    public TMP_Text pickupText;

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

        if (pickupText != null)
        {
            pickupText.gameObject.SetActive(true);
        }
    }
}

private void OnTriggerExit(Collider other)
{
    if (other.CompareTag("Player"))
    {
        InventoryPlayerInput.ClearNearbyPickup(this);

        if (pickupText != null)
            pickupText.gameObject.SetActive(false);
    }
}

    public void PickUp()
{
    if (itemData == null)
    {
        Debug.LogWarning("Не назначен Item Data у " + gameObject.name);
        return;
    }

    if (InventoryManager.Instance == null)
    {
        Debug.LogError("InventoryManager.Instance не найден!");
        return;
    }

    bool added = InventoryManager.Instance.TryAddItem(itemData, amount);

    if (!added)
    {
        Debug.Log("Недостаточно места в инвентаре!");
        return;
    }

    Debug.Log(
        "Подбор: " + itemData.itemName +
        " | Item Type: " + itemData.itemType +
        " | Weapon Kind: " + itemData.weaponKind
    );

    if (itemData.itemType == InventoryItemType.Weapon)
    {
        if (itemData.weaponKind == InventoryWeaponKind.None)
        {
            Debug.LogError("У оружия не указан Weapon Kind: " + itemData.itemName);
        }
        else if (WeaponSwitcher.Instance == null)
        {
            Debug.LogError("WeaponSwitcher.Instance не найден!");
        }
        else
        {
            bool unlocked = WeaponSwitcher.Instance.UnlockWeapon(itemData.weaponKind);

            Debug.Log("Результат разблокировки: " + unlocked);
        }
    }
    if (itemData.itemType == InventoryItemType.Ammo &&
    InventoryManager.Instance != null)
{
    InventoryManager.Instance.UpdateAmmoUI();
}
    Debug.Log("Подобран предмет: " + itemData.itemName);

    InventoryPlayerInput.ClearNearbyPickup(this);
   if (pickupText != null)
{
    pickupText.gameObject.SetActive(false);
}

Destroy(gameObject);
}
}
