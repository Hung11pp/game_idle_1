using IdleDefense.Core.Stats;
using TMPro;
using UnityEngine;

/// <summary>
/// Player HUD: HP, combat stats, equipment summary. Subscribes to inventory changes for HP recalc + refresh.
/// </summary>
[DisallowMultipleComponent]
public class PlayerUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text hpText;
    public TMP_Text statsText;
    public TMP_Text equippedText;

    private GameServices _services;
    private bool _initialized;

    public void Initialize(GameServices services)
    {
        if (_initialized)
        {
            return;
        }

        _services = services;
        if (_services?.Inventory != null)
        {
            _services.Inventory.EquipmentChanged += OnEquipmentChanged;
        }

        _initialized = true;
        OnEquipmentChanged();
    }

    private void OnDestroy()
    {
        if (_services != null && _services.Inventory != null)
        {
            _services.Inventory.EquipmentChanged -= OnEquipmentChanged;
        }
    }

    private void OnEquipmentChanged()
    {
        if (_services == null || _services.Player == null)
        {
            return;
        }

        _services.Player.RecalculateHpAfterStatChange(false);
        Refresh();
    }

    /// <summary>Full HUD refresh (safe to call every frame while playing).</summary>
    public void Refresh()
    {
        if (_services == null || _services.Player == null)
        {
            return;
        }

        float current = _services.Player.CurrentHp;
        float max = _services.Player.GetMaxHp();

        if (hpText != null)
        {
            hpText.text = "HP: " + Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
        }

        if (statsText != null)
        {
            float range = _services.Player.GetFinalStats().Get(StatId.AttackRange);
            statsText.text = "ATK: " + _services.Player.GetAttack().ToString("0.0")
                + "  SPD: " + _services.Player.GetAttackSpeed().ToString("0.00")
                + "  RNG: " + range.ToString("0.0");
        }

        if (equippedText != null)
        {
            equippedText.text = _services.Inventory.GetEquippedSummary();
        }
    }
}
