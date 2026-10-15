
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Размер сетки")]
    [Min(1)] public int columns = 8;
    [Min(1)] public int rows = 6;

    [Header("Интерфейс")]
    public GameObject inventoryPanel;
    public RectTransform itemLayer;
    public GameObject itemUIPrefab;

    [Header("Настройки ячеек")]
    public float cellSize = 64f;
    public float spacing = 4f;

    [Header("Предметы")]
    public List<InventoryItem> items = new List<InventoryItem>();

    public bool IsOpen { get; private set; }

    private readonly List<InventoryItemUI> itemViews =
        new List<InventoryItemUI>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    private void Start()
    {
        RefreshUI();
    }

    public void ToggleInventory()
    {
        SetInventoryOpen(!IsOpen);
    }

    public void SetInventoryOpen(bool open)
    {
        IsOpen = open;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(open);

        Cursor.visible = open;
        Cursor.lockState = open
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }

    public bool CanPlace(
        InventoryItemData data,
        int x,
        int y,
        InventoryItem ignoredItem = null)
    {
        if (data == null)
            return false;

        if (x < 0 || y < 0 ||
            x + data.width > columns ||
            y + data.height > rows)
            return false;

        foreach (InventoryItem item in items)
        {
            if (item == ignoredItem)
                continue;

            if (Overlaps(
                x, y, data.width, data.height,
                item.x, item.y,
                item.data.width, item.data.height))
                return false;
        }

        return true;
    }

    private bool Overlaps(
        int ax, int ay, int aw, int ah,
        int bx, int by, int bw, int bh)
    {
        return ax < bx + bw &&
               ax + aw > bx &&
               ay < by + bh &&
               ay + ah > by;
    }

    public bool TryAddItem(
        InventoryItemData data,
        int amount = 1)
    {
        if (data == null || amount <= 0)
            return false;

        // Сначала заполняем уже существующие стопки.
        if (data.maxStack > 1)
        {
            foreach (InventoryItem item in items)
            {
                if (item.data.itemId != data.itemId)
                    continue;

                int available = data.maxStack - item.amount;
                int transfer = Mathf.Min(available, amount);

                if (transfer > 0)
                {
                    item.amount += transfer;
                    amount -= transfer;
                }

                if (amount == 0)
                {
                    RefreshUI();
                    return true;
                }
            }
        }

        // Размещаем оставшиеся предметы в свободных ячейках.
        while (amount > 0)
        {
            int stackAmount = Mathf.Min(amount, data.maxStack);
            bool placed = false;

            for (int y = 0; y < rows && !placed; y++)
            {
                for (int x = 0; x < columns && !placed; x++)
                {
                    if (!CanPlace(data, x, y))
                        continue;

                    items.Add(new InventoryItem(
                        data, stackAmount, x, y));

                    amount -= stackAmount;
                    placed = true;
                }
            }

            // Если место закончилось, предметы не теряются.
            if (!placed)
            {
                RefreshUI();
                return false;
            }
        }

        RefreshUI();
        return true;
    }

    public bool TryMoveItem(
        InventoryItem item,
        int targetX,
        int targetY)
    {
        if (item == null || !items.Contains(item))
            return false;

        // Объединение одинаковых стопок.
        InventoryItem target = FindItemAt(targetX, targetY);

        if (target != null && target != item &&
            target.data.itemId == item.data.itemId &&
            target.data.maxStack > 1)
        {
            int free = target.data.maxStack - target.amount;
            int transfer = Mathf.Min(free, item.amount);

            if (transfer > 0)
            {
                target.amount += transfer;
                item.amount -= transfer;

                if (item.amount <= 0)
                    items.Remove(item);

                RefreshUI();
                return true;
            }
        }

        // Перемещение или обмен местами.
        int oldX = item.x;
        int oldY = item.y;

        if (CanPlace(item.data, targetX, targetY, item))
        {
            item.x = targetX;
            item.y = targetY;
            RefreshUI();
            return true;
        }

        // Если место занято, пробуем обмен.
        if (target != null && target != item &&
            target.data.width == item.data.width &&
            target.data.height == item.data.height &&
            CanPlace(item.data, targetX, targetY, item) == false)
        {
            items.Remove(target);

            if (CanPlace(item.data, targetX, targetY, item) &&
                CanPlace(target.data, oldX, oldY, target))
            {
                item.x = targetX;
                item.y = targetY;
                target.x = oldX;
                target.y = oldY;
                items.Add(target);
                RefreshUI();
                return true;
            }

            items.Add(target);
        }

        RefreshUI();
        return false;
    }

    public InventoryItem FindItemAt(int x, int y)
    {
        foreach (InventoryItem item in items)
        {
            if (x >= item.x &&
                x < item.x + item.data.width &&
                y >= item.y &&
                y < item.y + item.data.height)
            {
                return item;
            }
        }

        return null;
    }

    public void RefreshUI()
    {
        foreach (InventoryItemUI view in itemViews)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        itemViews.Clear();

        if (itemLayer == null || itemUIPrefab == null)
            return;

        foreach (InventoryItem item in items)
        {
            GameObject obj = Instantiate(itemUIPrefab, itemLayer);
            InventoryItemUI view = obj.GetComponent<InventoryItemUI>();

            if (view == null)
            {
                Debug.LogError(
                    "На ItemUIPrefab отсутствует InventoryItemUI!");
                Destroy(obj);
                continue;
            }

            view.Setup(item, this);
            itemViews.Add(view);
        }
    }
}
