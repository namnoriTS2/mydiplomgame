
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image icon;
    public TMP_Text amountText;

    private InventoryItem item;
    private InventoryManager manager;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

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

        if (icon != null)
            icon.sprite = item.data.icon;

        if (amountText != null)
        {
            amountText.text = item.data.maxStack > 1
                ? item.amount.ToString()
                : "";
        }

        float step = manager.cellSize + manager.spacing;

        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);

        rectTransform.sizeDelta = new Vector2(
            item.data.width * manager.cellSize +
                (item.data.width - 1) * manager.spacing,
            item.data.height * manager.cellSize +
                (item.data.height - 1) * manager.spacing
        );

        rectTransform.anchoredPosition = new Vector2(
            item.x * step,
            -item.y * step
        );
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (manager == null || !manager.IsOpen)
            return;

        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (manager == null || !manager.IsOpen)
            return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (manager == null)
            return;

        canvasGroup.blocksRaycasts = true;

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
            float step = manager.cellSize + manager.spacing;

            int x = Mathf.FloorToInt(localPoint.x / step);
            int y = Mathf.FloorToInt(-localPoint.y / step);

            manager.TryMoveItem(item, x, y);
        }
        else
        {
            manager.RefreshUI();
        }
    }
}
