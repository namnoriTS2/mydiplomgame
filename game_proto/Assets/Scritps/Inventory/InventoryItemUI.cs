
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("UI")]
    public Image icon;
    public TMP_Text amountText;

    private InventoryItem item;
    private InventoryManager manager;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 dragOffset;
    private Vector2 originalPosition;

    private bool isDragging;

    // =====================================================
    // НАСТРОЙКА ПРЕДМЕТА
    // =====================================================

    public void Setup(
        InventoryItem inventoryItem,
        InventoryManager inventoryManager)
    {
        item = inventoryItem;
        manager = inventoryManager;

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Если Image не назначен вручную,
        // используем Image самого объекта.
        if (icon == null)
            icon = GetComponent<Image>();

        // Если текст не назначен вручную,
        // ищем его среди дочерних объектов.
        if (amountText == null)
            amountText = GetComponentInChildren<TMP_Text>(true);

        if (icon != null)
        {
            icon.sprite = item.data.icon;
            icon.preserveAspect = true;
            icon.raycastTarget = true;
        }

        UpdateAmountText();
        UpdateSize();
        UpdatePosition();
    }

    // =====================================================
    // КОЛИЧЕСТВО ПРЕДМЕТОВ
    // =====================================================

    public void UpdateAmountText()
    {
        if (amountText == null)
            return;

        if (item.data.maxStack > 1 && item.amount > 1)
            amountText.text = item.amount.ToString();
        else
            amountText.text = "";
    }

    // =====================================================
    // РАЗМЕР ПРЕДМЕТА
    // =====================================================

    public void UpdateSize()
    {
        if (item == null || item.data == null)
            return;

        float width =
            item.data.width * manager.cellWidth +
            (item.data.width - 1) * manager.spacingX;

        float height =
            item.data.height * manager.cellHeight +
            (item.data.height - 1) * manager.spacingY;

        rectTransform.sizeDelta = new Vector2(
            width,
            height
        );
    }

    // =====================================================
    // ПОЛОЖЕНИЕ ПРЕДМЕТА
    // =====================================================

    public void UpdatePosition()
    {
        if (item == null || manager == null)
            return;

        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);

        rectTransform.anchoredPosition = new Vector2(
            item.x * manager.CellStepX,
            -item.y * manager.CellStepY
        );
    }

    // =====================================================
    // НАЧАЛО ПЕРЕТАСКИВАНИЯ
    // =====================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (manager == null || !manager.IsOpen)
            return;

        if (item == null)
            return;

        originalPosition = rectTransform.anchoredPosition;

        // Запоминаем смещение курсора относительно предмета.
        if (RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                manager.itemLayer,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            dragOffset =
                originalPosition - localPoint;
        }

        isDragging = true;

        // Предмет рисуется поверх остальных.
        transform.SetAsLastSibling();

        // Позволяем UI получать события мыши под предметом.
        canvasGroup.blocksRaycasts = false;
    }

    // =====================================================
    // ПЕРЕТАСКИВАНИЕ
    // =====================================================

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || manager == null)
            return;

        if (!manager.IsOpen)
            return;

        if (RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                manager.itemLayer,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            rectTransform.anchoredPosition =
                localPoint + dragOffset;
        }
    }

    // =====================================================
    // ЗАВЕРШЕНИЕ ПЕРЕТАСКИВАНИЯ
    // =====================================================

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (manager == null || item == null)
            return;

        if (!manager.IsOpen)
        {
            manager.RefreshUI();
            return;
        }

        if (RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                manager.itemLayer,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            // Получаем предполагаемое положение
            // верхнего левого угла предмета.
            Vector2 itemPosition =
                localPoint + dragOffset;

            int targetX = Mathf.FloorToInt(
                itemPosition.x / manager.CellStepX);

            int targetY = Mathf.FloorToInt(
                -itemPosition.y / manager.CellStepY);

            bool success = manager.TryMoveItem(
                item,
                targetX,
                targetY
            );

            if (!success)
            {
                rectTransform.anchoredPosition =
                    originalPosition;
            }
        }
        else
        {
            manager.RefreshUI();
        }
    }
}
