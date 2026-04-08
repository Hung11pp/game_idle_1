using IdleDefense.GameServices.Presentation;
using TMPro;
using UnityEngine;

/// <summary>
/// Presentation: inventory UI bound to Core InventoryState (created in GameServices).
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [Header("References")]
    public GameServices Services;

    [Header("UI")]
    public TMP_Text inventoryText;

    private void Awake()
    {
        if (Services == null)
        {
            Services = FindObjectOfType<GameServices>();
        }
    }

    private void OnEnable()
    {
        if (Services?.Inventory != null)
        {
            Services.Inventory.EquipmentChanged += UpdateUI;
        }
    }

    private void OnDisable()
    {
        if (Services?.Inventory != null)
        {
            Services.Inventory.EquipmentChanged -= UpdateUI;
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (inventoryText == null || Services == null || Services.Inventory == null)
        {
            return;
        }

        inventoryText.text = Services.Inventory.GetEquippedSummary();
    }
}
