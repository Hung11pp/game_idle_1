using IdleDefense.Core.Math;
using IdleDefense.GameServices.Presentation;
using IdleDefense.GameServices.Systems.EventBus;
using UnityEngine;

/// <summary>
/// Publishes <see cref="EnemyDiedEvent"/> so systems like <c>LootSystem</c> can roll drops.
/// Call from the enemy death sequence before returning to the pool.
/// </summary>
public sealed class LootOnDeath : MonoBehaviour
{
    private GameServices _services;
    private bool _isBoss;
    private int _waveNumber;

    public void Configure(GameServices services, bool isBoss, int waveNumber)
    {
        _services = services;
        _isBoss = isBoss;
        _waveNumber = waveNumber;
    }

    public void PublishDeathEvent()
    {
        if (_services == null)
        {
            _services = FindObjectOfType<GameServices>();
        }

        if (_services != null && _services.Events != null)
        {
            Vector3 p = transform.position;
            Position3 pos = new Position3(p.x, p.y, p.z);
            // Publisher: only the bus is needed — no reference to LootSystem or UI.
            _services.Events.EnemyDied.Publish(new EnemyDiedEvent(pos, _isBoss, _waveNumber));
        }
    }
}
