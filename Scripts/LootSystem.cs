using IdleDefense.GameServices.Presentation;
using IdleDefense.GameServices.Systems.EventBus;
using TMPro;
using UnityEngine;

/// <summary>
/// Presentation: reacts to Core events and delegates rolls to LootDropService.
/// </summary>
public class LootSystem : MonoBehaviour
{
    [Header("References")]
    public GameServices Services;

    [Header("UI")]
    public TMP_Text lootText;

    private void Awake()
    {
        if (Services == null)
        {
            Services = FindObjectOfType<GameServices>();
        }
    }

    private void OnEnable()
    {
        if (Services?.Events != null)
        {
            // Listener: subscribe to channels — no direct call from enemy or loot code.
            Services.Events.EnemyDied.OnEvent += HandleEnemyDied;
            Services.Events.LootDropped.OnEvent += HandleLootDropped;
        }
    }

    private void OnDisable()
    {
        if (Services?.Events != null)
        {
            Services.Events.EnemyDied.OnEvent -= HandleEnemyDied;
            Services.Events.LootDropped.OnEvent -= HandleLootDropped;
        }
    }

    private void HandleEnemyDied(EnemyDiedEvent e)
    {
        if (Services == null || Services.Loot == null || Services.Inventory == null)
        {
            ShowLootMessage("No loot (missing services)");
            return;
        }

        bool dropped = Services.Loot.TryDropLoot(e.IsBoss, e.WaveNumber, Services.Inventory, Services.Events);
        if (!dropped)
        {
            ShowLootMessage("No loot dropped");
        }
    }

    private void HandleLootDropped(LootDroppedEvent e)
    {
        if (e.Item == null)
        {
            return;
        }

        string equipNote = e.WasEquipped ? "" : " (not equipped)";
        ShowLootMessage("Loot: " + e.Item.GetSummary() + equipNote);
    }

    private void ShowLootMessage(string message)
    {
        if (lootText != null)
        {
            lootText.text = message;
        }
    }
}
