
using UnityEngine;

public enum InventoryItemType
{
    Weapon,
    Ammo,
    Medical,
    Key,
    Quest,
    Resource
}

[CreateAssetMenu(
    fileName = "NewInventoryItem",
    menuName = "Patient Zero/Inventory Item"
)]
public class InventoryItemData : ScriptableObject
{
    [Header("Основная информация")]
    public string itemId;
    public string itemName;
    [TextArea] public string description;

    [Header("Внешний вид")]
    public Sprite icon;

    [Header("Тип предмета")]
    public InventoryItemType itemType;

    [Header("Размер в сетке")]
    [Min(1)] public int width = 1;
    [Min(1)] public int height = 1;

    [Header("Стопки")]
    [Min(1)] public int maxStack = 1;
}
