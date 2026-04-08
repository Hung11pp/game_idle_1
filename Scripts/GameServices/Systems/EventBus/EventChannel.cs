using System;

namespace IdleDefense.GameServices.Systems.EventBus
{
    /// <summary>
    /// One typed event stream: listeners subscribe; producers call <see cref="Publish"/>.
    /// This is the building block of the <see cref="GameEventBus"/> pattern.
    /// </summary>
    /// <remarks>
    /// <b>Subscribe</b> (in OnEnable): <c>channel.OnEvent += YourHandler;</c><br/>
    /// <b>Unsubscribe</b> (in OnDisable): <c>channel.OnEvent -= YourHandler;</c><br/>
    /// <b>Publish</b> (where the thing happens): <c>channel.Publish(new SomeEvent(...));</c>
    /// </remarks>
    public sealed class EventChannel<T> where T : struct
    {
        /// <summary>Fired when <see cref="Publish"/> is called.</summary>
        public event Action<T> OnEvent;

        /// <summary>Notify all subscribers (usually called from one place — e.g. enemy death).</summary>
        public void Publish(T payload)
        {
            OnEvent?.Invoke(payload);
        }

        /// <summary>Remove every listener (e.g. when clearing the bus on shutdown).</summary>
        public void Clear()
        {
            OnEvent = null;
        }
    }
}
