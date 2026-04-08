using IdleDefense.Core.Enemy;
using UnityEngine;

/// <summary>
/// Moves toward the player using stats from <see cref="EnemyState"/>.
/// </summary>
public sealed class EnemyMovement : MonoBehaviour
{
    private EnemyState _state;
    private Health _health;
    private PlayerController _target;

    public void Configure(EnemyState state, Health health, PlayerController target)
    {
        _state = state;
        _health = health;
        _target = target;
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

        if (distance > 0.01f)
        {
            Vector3 moveDirection = direction.normalized;
            transform.position += moveDirection * _state.MoveSpeed * Time.deltaTime;
            transform.forward = moveDirection;
        }
    }
}
