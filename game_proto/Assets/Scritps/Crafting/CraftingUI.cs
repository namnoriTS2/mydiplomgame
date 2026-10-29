
using UnityEngine;
using TMPro;

public class CraftingUI : MonoBehaviour
{
    public static bool IsCraftingOpen { get; private set; }

    [Header("Панель крафта")]
    public GameObject craftingPanel;
    public TMP_Text resourcesText;
    public TMP_Text craftedItemsText;

    private void Start()
    {
        IsCraftingOpen = false;

        if (craftingPanel != null)
            craftingPanel.SetActive(false);

        RefreshUI();
    }

    public void Open()
    {
        if (craftingPanel == null ||
            CraftingManager.Instance == null ||
            IsCraftingOpen)
            return;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.SetInventoryOpen(false);

        IsCraftingOpen = true;
        craftingPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshUI();
    }

    public void Close()
    {
        if (craftingPanel != null)
            craftingPanel.SetActive(false);

        IsCraftingOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void CraftAmmo()
    {
        CraftingManager.Instance?.CraftAmmo();
        RefreshUI();
    }

    public void CraftMedkit()
    {
        CraftingManager.Instance?.CraftMedkit();
        RefreshUI();
    }

    public void RefreshUI()
    {
        CraftingManager manager = CraftingManager.Instance;

        if (manager == null)
            return;

        if (resourcesText != null)
        {
            resourcesText.text =
                "Порох: " + manager.Count(manager.gunpowderData) + "\n" +
                "Металлические детали: " + manager.Count(manager.metalPartsData) + "\n" +
                "Ткань: " + manager.Count(manager.clothData) + "\n" +
                "Химические компоненты: " + manager.Count(manager.chemicalsData);
        }

        if (craftedItemsText != null)
        {
            craftedItemsText.text =
                "Патроны для пистолета: " + manager.Count(manager.pistolAmmoData) + "\n" +
                "Аптечки: " + manager.Count(manager.medkitData);
        }
    }
}
