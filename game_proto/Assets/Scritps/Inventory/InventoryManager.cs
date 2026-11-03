
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Размер сетки")]
    [Min(1)] public int columns = 12;
    [Min(1)] public int rows = 7;

    [Header("Интерфейс")]
    public GameObject inventoryPanel;
    public RectTransform itemLayer;
    public GameObject itemUIPrefab;

    [Header("Размер ячеек")]
    public float cellWidth = 122f;
    public float cellHeight = 94f;
    public float spacingX = 4.8f;
    public float spacingY = 4.5f;

    [Header("Предметы")]
    public List<InventoryItem> items = new List<InventoryItem>();

    public bool IsOpen { get; private set; }

    private readonly List<InventoryItemUI> itemViews =
        new List<InventoryItemUI>();

    private bool isRefreshing;

    // Шаг между началами соседних клеток.
    public float CellStepX => cellWidth + spacingX;
    public float CellStepY => cellHeight + spacingY;

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

        IsOpen = false;
    }

    private void Start()
    {
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // =====================================================
    // ОТКРЫТИЕ И ЗАКРЫТИЕ
    // =====================================================

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

    // =====================================================
    // ПРОВЕРКА ПЕРЕСЕЧЕНИЯ
    // =====================================================

    private bool Overlaps(
        int ax, int ay, int aw, int ah,
        int bx, int by, int bw, int bh)
    {
        return ax < bx + bw &&
               ax + aw > bx &&
               ay < by + bh &&
               ay + ah > by;
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
            if (item == null || item.data == null)
                continue;

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

    private bool CanPlaceIgnoring(
        InventoryItemData data,
        int x,
        int y,
        InventoryItem ignoredA,
        InventoryItem ignoredB)
    {
        if (data == null)
            return false;

        if (x < 0 || y < 0 ||
            x + data.width > columns ||
            y + data.height > rows)
            return false;

        foreach (InventoryItem item in items)
        {
            if (item == null || item.data == null)
                continue;

            if (item == ignoredA || item == ignoredB)
                continue;

            if (Overlaps(
                x, y, data.width, data.height,
                item.x, item.y,
                item.data.width, item.data.height))
                return false;
        }

        return true;
    }

    // =====================================================
    // ПОИСК СВОБОДНОГО МЕСТА
    // =====================================================

    private bool FindFreePosition(
        InventoryItemData data,
        bool[,] occupied,
        out int foundX,
        out int foundY)
    {
        for (int y = 0; y <= rows - data.height; y++)
        {
            for (int x = 0; x <= columns - data.width; x++)
            {
                bool free = true;

                for (int dx = 0; dx < data.width && free; dx++)
                {
                    for (int dy = 0; dy < data.height; dy++)
                    {
                        if (occupied[x + dx, y + dy])
                        {
                            free = false;
                            break;
                        }
                    }
                }

                if (!free)
                    continue;

                foundX = x;
                foundY = y;
                return true;
            }
        }

        foundX = -1;
        foundY = -1;
        return false;
    }

    private void MarkOccupied(
        bool[,] occupied,
        int x,
        int y,
        int width,
        int height)
    {
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                occupied[x + dx, y + dy] = true;
            }
        }
    }

    // =====================================================
    // ПРОВЕРКА ВОЗМОЖНОСТИ ПОДБОРА
    // =====================================================

    public bool CanAddItem(InventoryItemData data, int amount)
    {
        if (data == null || amount <= 0 ||
            data.width <= 0 || data.height <= 0 ||
            data.maxStack <= 0 ||
            data.width > columns || data.height > rows)
            return false;

        int remaining = amount;

        // Учитываем свободные места в существующих стопках.
        if (data.maxStack > 1)
        {
            foreach (InventoryItem item in items)
            {
                if (item == null || item.data == null)
                    continue;

                if (item.data.itemId != data.itemId)
                    continue;

                int free = Mathf.Max(
                    0, item.data.maxStack - item.amount);

                remaining -= free;

                if (remaining <= 0)
                    return true;
            }
        }

        // Создаём временную карту занятых клеток.
        bool[,] occupied = new bool[columns, rows];

        foreach (InventoryItem item in items)
        {
            if (item == null || item.data == null)
                continue;

            for (int x = item.x;
                 x < item.x + item.data.width; x++)
            {
                for (int y = item.y;
                     y < item.y + item.data.height; y++)
                {
                    if (x >= 0 && x < columns &&
                        y >= 0 && y < rows)
                    {
                        occupied[x, y] = true;
                    }
                }
            }
        }

        // Проверяем все новые стопки.
        while (remaining > 0)
        {
            if (!FindFreePosition(
                data, occupied, out int x, out int y))
                return false;

            MarkOccupied(
                occupied, x, y,
                data.width, data.height);

            remaining -= Mathf.Min(
                remaining, data.maxStack);
        }

        return true;
    }

    // =====================================================
    // ДОБАВЛЕНИЕ ПРЕДМЕТА
    // =====================================================

    public bool TryAddItem(
        InventoryItemData data,
        int amount = 1)
    {
        if (!CanAddItem(data, amount))
        {
            Debug.Log("Недостаточно места в инвентаре!");
            return false;
        }

        int remaining = amount;

        // Сначала заполняем уже существующие стопки.
        if (data.maxStack > 1)
        {
            foreach (InventoryItem item in items)
            {
                if (item == null || item.data == null)
                    continue;

                if (item.data.itemId != data.itemId)
                    continue;

                int free = item.data.maxStack - item.amount;

                if (free <= 0)
                    continue;

                int transfer = Mathf.Min(free, remaining);

                item.amount += transfer;
                remaining -= transfer;

                if (remaining <= 0)
                    break;
            }
        }

        // Создаём новые стопки.
        while (remaining > 0)
        {
            int stackAmount = Mathf.Min(
                remaining, data.maxStack);

            if (!FindPositionForNewItem(
                data, out int x, out int y))
            {
                Debug.LogError(
                    "Ошибка размещения предмета: " + data.itemName);
                RefreshUI();
                return false;
            }

            items.Add(new InventoryItem(
                data, stackAmount, x, y));

            remaining -= stackAmount;
        }

        RefreshUI();
        UpdateAmmoUI();

        return true;
    }

    private bool FindPositionForNewItem(
        InventoryItemData data,
        out int foundX,
        out int foundY)
    {
        for (int y = 0; y <= rows - data.height; y++)
        {
            for (int x = 0; x <= columns - data.width; x++)
            {
                if (CanPlace(data, x, y))
                {
                    foundX = x;
                    foundY = y;
                    return true;
                }
            }
        }

        foundX = -1;
        foundY = -1;
        return false;
    }

    // =====================================================
    // ПЕРЕМЕЩЕНИЕ И ОБЪЕДИНЕНИЕ
    // =====================================================

    public bool TryMoveItem(
        InventoryItem item,
        int targetX,
        int targetY)
    {
        if (item == null || !items.Contains(item))
            return false;

        InventoryItem target = FindItemAt(
            targetX, targetY);

        // Объединение одинаковых стопок.
        if (target != null && target != item &&
            target.data.itemId == item.data.itemId &&
            target.data.maxStack > 1)
        {
            int free = target.data.maxStack - target.amount;

            if (free > 0)
            {
                int transfer = Mathf.Min(free, item.amount);

                target.amount += transfer;
                item.amount -= transfer;

                if (item.amount <= 0)
                    items.Remove(item);

                RefreshUI();
                UpdateAmmoUI();

                return true;
            }
        }

        int oldX = item.x;
        int oldY = item.y;

        // Обычное перемещение.
        if (CanPlace(item.data, targetX, targetY, item))
        {
            item.x = targetX;
            item.y = targetY;

            RefreshUI();
            return true;
        }

        // Обмен предметами одинакового размера.
        if (target != null && target != item)
        {
            bool sameSize =
                item.data.width == target.data.width &&
                item.data.height == target.data.height;

            if (sameSize &&
                CanPlaceIgnoring(
                    item.data, targetX, targetY, item, target) &&
                CanPlaceIgnoring(
                    target.data, oldX, oldY, item, target))
            {
                item.x = targetX;
                item.y = targetY;

                target.x = oldX;
                target.y = oldY;

                RefreshUI();
                return true;
            }
        }

        // Перемещение невозможно.
        RefreshUI();
        return false;
    }

    // =====================================================
    // ПОИСК ПРЕДМЕТА В КЛЕТКЕ
    // =====================================================

    public InventoryItem FindItemAt(int x, int y)
    {
        foreach (InventoryItem item in items)
        {
            if (item == null || item.data == null)
                continue;

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

    // =====================================================
    // ОБНОВЛЕНИЕ ИНТЕРФЕЙСА
    // =====================================================

    public void RefreshUI()
    {
        if (isRefreshing)
            return;

        isRefreshing = true;

        foreach (InventoryItemUI view in itemViews)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        itemViews.Clear();

        if (itemLayer == null || itemUIPrefab == null)
        {
            isRefreshing = false;
            return;
        }

        foreach (InventoryItem item in items)
        {
            if (item == null || item.data == null)
                continue;

            GameObject obj = Instantiate(
                itemUIPrefab, itemLayer);

            InventoryItemUI view =
                obj.GetComponent<InventoryItemUI>();

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

        isRefreshing = false;
    }

    // =====================================================
    // ПАТРОНЫ
    // =====================================================

    public int GetAmmoCount(InventoryWeaponKind kind)
    {
        int total = 0;

        foreach (InventoryItem item in items)
        {
            if (item == null || item.data == null)
                continue;

            if (item.data.itemType == InventoryItemType.Ammo &&
                item.data.weaponKind == kind)
            {
                total += item.amount;
            }
        }

        return total;
    }

    public int ConsumeAmmo(
        InventoryWeaponKind kind,
        int amount)
    {
        if (amount <= 0)
            return 0;

        int remaining = amount;

        for (int i = items.Count - 1;
             i >= 0 && remaining > 0; i--)
        {
            InventoryItem item = items[i];

            if (item == null || item.data == null)
                continue;

            if (item.data.itemType != InventoryItemType.Ammo ||
                item.data.weaponKind != kind)
                continue;

            int taken = Mathf.Min(item.amount, remaining);

            item.amount -= taken;
            remaining -= taken;

            if (item.amount <= 0)
                items.RemoveAt(i);
        }

        int consumed = amount - remaining;

        if (consumed > 0)
        {
            RefreshUI();
            UpdateAmmoUI();
        }

        return consumed;
    }

    public void UpdateAmmoUI()
    {
        if (WeaponSwitcher.Instance == null)
            return;

        GameObject weapon =
            WeaponSwitcher.Instance.GetCurrentWeapon();

        if (weapon == null)
            return;

        Shotgun shotgun = weapon.GetComponent<Shotgun>();

        if (shotgun != null)
        {
            shotgun.UpdateUIAmmo();
            return;
        }

        Gun gun = weapon.GetComponent<Gun>();

        if (gun != null)
        {
            gun.UpdateUIAmmo();
            return;
        }

        GunPistol pistol = weapon.GetComponent<GunPistol>();

        if (pistol != null)
            pistol.UpdateUIAmmo();
    }

    // =====================================================
    // РЕСУРСЫ И КРАФТ
    // =====================================================

    public int CountItemById(string itemId)
    {
        int total = 0;

        foreach (InventoryItem item in items)
        {
            if (item == null || item.data == null)
                continue;

            if (item.data.itemId == itemId)
                total += item.amount;
        }

        return total;
    }

    public bool HasItems(string itemId, int amount)
    {
        return CountItemById(itemId) >= amount;
    }

    public bool ConsumeItemsById(
        string itemId,
        int amount)
    {
        if (amount <= 0 || !HasItems(itemId, amount))
            return false;

        int remaining = amount;

        for (int i = items.Count - 1;
             i >= 0 && remaining > 0; i--)
        {
            InventoryItem item = items[i];

            if (item == null || item.data == null)
                continue;

            if (item.data.itemId != itemId)
                continue;

            int taken = Mathf.Min(item.amount, remaining);

            item.amount -= taken;
            remaining -= taken;

            if (item.amount <= 0)
                items.RemoveAt(i);
        }

        RefreshUI();
        UpdateAmmoUI();

        return remaining == 0;
    }
}
