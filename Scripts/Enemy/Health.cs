using IdleDefense.Core.Enemy;
using IdleDefense.GameServices.CoreServices.Combat;
using IdleDefense.GameServices.Presentation;
using UnityEngine;

/// <summary>
/// Enemy HP and damage application; invokes a single death callback when health reaches zero.
/// </summary>
public sealed class Health : MonoBehaviour
{
    private EnemyState _state;
    private GameServices _services;
    private System.Action _onDeath;
    private bool _alive;

    public void Configure(EnemyState state, GameServices services, System.Action onDeath)
    {
        _state = state;
        _services = services;
        _onDeath = onDeath;
    }

    public void ResetForSpawn()
    {
        _alive = true;
    }

    public bool IsAlive()
    {
        return _alive && gameObject.activeInHierarchy && _state != null && _state.IsAlive;
    }

    public void TakeDamage(float damageAmount)
    {
        if (!_alive || _state == null)
        {
            return;
        }

        if (_services == null)
        {
            _services = FindObjectOfType<GameServices>();
        }

        CombatService combat = _services != null ? _services.Combat : new CombatService();
        float hp = _state.CurrentHealth;

        if (combat.ApplyDamageToHealth(ref hp, damageAmount))
        {
            _state.CurrentHealth = 0f;
            _alive = false;
            _onDeath?.Invoke();
        }
        else
        {
            _state.CurrentHealth = hp;
        }
    }
}
