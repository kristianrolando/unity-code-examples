using System;
using System.Collections.Generic;

namespace Game.Core.EventBus
{
    /// <summary>
    /// A non-generic, global event bus that uses string-based event IDs.
    /// All enum-based event buses will map to this internally.
    /// </summary>
    public static class EventBus
    {
        // Internal storage: eventId(string) -> callbacks
        private static readonly Dictionary<string, Action<object[]>> _events = new();

        /// <summary>
        /// Subscribes a callback to a specific event identified by string.
        /// </summary>
        /// <param name="eventId">The event identifier (string).</param>
        /// <param name="callback">The method to be called when the event is triggered.</param>
        public static void Subscribe(string eventId, Action<object[]> callback)
        {
            if (string.IsNullOrEmpty(eventId) || callback == null)
                return;

            if (!_events.ContainsKey(eventId))
                _events[eventId] = callback;
            else
                _events[eventId] += callback;
        }

        /// <summary>
        /// Unsubscribes a callback from a specific event.
        /// </summary>
        /// <param name="eventId">The event identifier (string).</param>
        /// <param name="callback">The callback method to remove.</param>
        public static void Unsubscribe(string eventId, Action<object[]> callback)
        {
            if (string.IsNullOrEmpty(eventId) || callback == null)
                return;

            if (_events.TryGetValue(eventId, out var existing))
            {
                existing -= callback;

                if (existing == null)
                    _events.Remove(eventId);
                else
                    _events[eventId] = existing;
            }
        }

        /// <summary>
        /// Triggers an event by its string ID and invokes all registered callbacks.
        /// </summary>
        /// <param name="eventId">The event identifier (string).</param>
        /// <param name="args">Optional arguments passed to subscribers.</param>
        public static void Trigger(string eventId, params object[] args)
        {
            if (string.IsNullOrEmpty(eventId))
                return;

            if (_events.TryGetValue(eventId, out var action))
            {
                action?.Invoke(args);
            }
        }

        /// <summary>
        /// Removes all events and their listeners.
        /// </summary>
        public static void Clear()
        {
            _events.Clear();
        }
    }

    /// <summary>
    /// Type-safe enum wrapper around the string-based EventBus.
    /// Provides a strongly-typed interface for subscribing, unsubscribing, and triggering events.
    /// </summary>
    /// <typeparam name="TEnum">The enum type used as event identifiers.</typeparam>
    public static class EventBus<TEnum> where TEnum : Enum
    {
        /// <summary>
        /// Converts the enum value to a unique string key.
        /// Namespaced with the enum’s full type name to avoid collisions
        /// between enums that share identical member names.
        /// </summary>
        private static string ToKey(TEnum eventId)
        {
            // Example key: "MyGame.Events.GameEvent/PlayerDied"
            return $"{typeof(TEnum).FullName}/{eventId}";
        }

        /// <summary>
        /// Subscribes a callback using an enum event ID.
        /// </summary>
        /// <param name="eventId">The enum value identifying the event.</param>
        /// <param name="callback">The method to invoke when the event is triggered.</param>
        public static void Subscribe(TEnum eventId, Action<object[]> callback)
        {
            EventBus.Subscribe(ToKey(eventId), callback);
        }

        /// <summary>
        /// Unsubscribes a callback using an enum event ID.
        /// </summary>
        /// <param name="eventId">The enum value identifying the event.</param>
        /// <param name="callback">The callback method to remove.</param>
        public static void Unsubscribe(TEnum eventId, Action<object[]> callback)
        {
            EventBus.Unsubscribe(ToKey(eventId), callback);
        }

        /// <summary>
        /// Triggers an event using an enum event ID.
        /// </summary>
        /// <param name="eventId">The enum value identifying the event.</param>
        /// <param name="args">Optional parameters passed to the callbacks.</param>
        public static void Trigger(TEnum eventId, params object[] args)
        {
            EventBus.Trigger(ToKey(eventId), args);
        }

        /// <summary>
        /// Clears all registered events.
        /// Note:
        /// - This simple implementation doesn’t have direct access
        ///   to EventBus’s internal dictionary, so it can’t remove
        ///   only the events associated with this specific enum type.
        /// - Use EventBus.Clear() to wipe all events globally, or
        ///   implement EventBusExtended for per-enum cleanup.
        /// </summary>
        [Obsolete("EventBus<TEnum>.Clear() cannot selectively clear events per enum type. Use EventBus.Clear() for a global wipe.")]
        public static void Clear()
        {
            EventBus.Clear();
        }
    }
}
