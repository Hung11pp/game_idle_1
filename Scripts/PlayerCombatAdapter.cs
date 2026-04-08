using IdleDefense.Core.Stats;
using IdleDefense.GameServices.CoreServices.Combat;
using UnityEngine;

/// <summary>
/// Unity-side auto-attack: cooldown stepping + nearest target resolution. Core rules live in CombatService / PlayerState.
/// </summary>
[DisallowMultipleComponent]
public class PlayerCombatAdapter : MonoBehaviour
{
    [Header("Combat (cooldown state; stepped by CombatService)")]
    public float attackCooldown;

    private GameServices _services;
    private EnemyPool _enemyPool;
    private Transform _playerTransform;
    private bool _initialized;

    public void Initialize(GameServices services, EnemyPool enemyPool, Transform playerTransform)
    {
        _services = services;
        _enemyPool = enemyPool;
        _playerTransform = playerTransform;
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized || _services == null || _services.Player == null)
        {
            return;
        }

        if (_services.Player.IsDead)
        {
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.gameRunning)
        {
            return;
        }

        HandleAutoAttack();
    }

    private void HandleAutoAttack()
    {
        CombatService combat = _services.Combat;
        combat.TickCooldown(ref attackCooldown, Time.deltaTime);
        if (!combat.IsAttackReady(attackCooldown))
        {
            return;
        }

        EnemyController target = FindNearestEnemyInRange();
        if (target == null)
        {
            return;
        }

        float damage = _services.Player.GetAttack();
        target.TakeDamage(damage);
        combat.BeginAttackCooldown(ref attackCooldown, combat.SecondsPerAttack(_services.Player.GetAttackSpeed()));
    }

    private EnemyController FindNearestEnemyInRange()
    {
        if (_enemyPool == null)
        {
            _enemyPool = FindObjectOfType<EnemyPool>();
        }

        if (_enemyPool == null)
        {
            return null;
        }

        float range = _services.Player.GetFinalStats().Get(StatId.AttackRange);
        return _enemyPool.GetNearestActiveEnemy(_playerTransform.position, range);
    }
}
