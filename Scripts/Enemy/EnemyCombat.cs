using IdleDefense.Core.Enemy;
using IdleDefense.GameServices.CoreServices.Combat;
using IdleDefense.GameServices.Presentation;
using UnityEngine;

/// <summary>
/// Contact-range periodic damage to the player.
/// </summary>
public sealed class EnemyCombat : MonoBehaviour
{
    private EnemyState _state;
    private Health _health;
    private PlayerController _target;
    private GameServices _services;
    private float _damageTimer;

    public void Configure(EnemyState state, Health health, PlayerController target, GameServices services)
    {
        _state = state;
        _health = health;
        _target = target;
        _services = services;
    }

    public void ResetForSpawn()
    {
        _damageTimer = 0f;
    }

    private void Update()
    {
        if (_state == null || _health == null || !_health.IsAlive())
        {
            return;
        }

        if (_target == null || _target.IsDead())
        {
            return;
        }

        Vector3 direction = _target.transform.position - transform.position;
        float distance = direction.magnitude;
        bool inContact = distance <= _state.ContactRange;

        if (_services == null)
        {
            _services = FindObjectOfType<GameServices>();
        }

        CombatService combat = _services != null ? _services.Combat : new CombatService();
        if (combat.TickPeriodicDamage(ref _damageTimer, Time.deltaTime, inContact, _state.DamageTickSeconds))
        {
            _target.TakeDamage(_state.ContactDamage);
        }
    }
}
