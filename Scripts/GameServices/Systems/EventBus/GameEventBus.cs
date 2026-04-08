namespace IdleDefense.GameServices.Systems.EventBus
{
    /// <summary>
    /// Application-wide event bus: holds one <see cref="EventChannel{T}"/> per event type.
    /// Keeps systems decoupled — publishers do not need references to listeners.
    /// </summary>
    /// <remarks>
    /// Create a single instance (e.g. on <c>GameServices</c>) and pass it where needed,
    /// or inject it into services that only <see cref="EventChannel{T}.Publish"/>.
    /// </remarks>
    public sealed class GameEventBus
    {
        /// <summary>Enemy reached zero HP (loot, VFX, stats — subscribe here).</summary>
        public readonly EventChannel<EnemyDiedEvent> EnemyDied = new EventChannel<EnemyDiedEvent>();

        /// <summary>Loot was generated after a drop roll (UI, sounds).</summary>
        public readonly EventChannel<LootDroppedEvent> LootDropped = new EventChannel<LootDroppedEvent>();

        /// <summary>Stops all listeners — call from <c>OnDestroy</c> on the owner to avoid leaks.</summary>
        public void ClearAllSubscriptions()
        {
            EnemyDied.Clear();
            LootDropped.Clear();
        }
    }
}
