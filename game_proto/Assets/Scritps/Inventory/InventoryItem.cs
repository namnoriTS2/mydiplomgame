
using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public InventoryItemData data;
    public int amount;
    public int x;
    public int y;

    public InventoryItem(
        InventoryItemData itemData,
        int itemAmount,
        int posX,
        int posY)
    {
        data = itemData;
        amount = itemAmount;
        x = posX;
        y = posY;
    }
}
