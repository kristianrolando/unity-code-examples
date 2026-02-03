// DebugX.cs
// -----------------------------------------------------------------------------
// Developer summary:
// This is a debug/telemetry helper for Unity that mirrors UnityEngine.Debug but
// adds extra features: channels (tag-based logging), color presets, timestamps,
// log-once / rate-limited logs, JSON dumps, quick perf/memory snapshots, small
// profiler helpers, and simple drawing helpers.
//
// Important notes:
// - All public methods are wrapped with [Conditional("UNITY_EDITOR"),
//   "DEVELOPMENT_BUILD"], so they are STRIPPED from release builds by default.
//   That means calling them in Release/Store builds will do nothing.
// - There is a global enable/disable switch (SetEnabled) and per-channel
//   enabling/disabling.
// - This is intended for runtime debugging during development, not for
//   production analytics.
//
// Typical usage:
//   DebugX.Log("Player spawned");
//   DebugX.LogWarning("Low HP", "Player");
//   DebugX.LogError(ex, "NETWORK");
//   DebugX.LogJson(myData);
//   using (DebugX.Measure("HeavyLoop")) { /* ... */ }
//
// At the bottom of this file there is a "Quick usage" section with examples.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics; // for [Conditional]
using UnityEngine;
using UnityEngine.Profiling;

namespace Game.Utilities
{
    public static class DebugX
    {
        // ---------------------------------------------------------------------
        // Global toggles
        // ---------------------------------------------------------------------
        private static bool s_enabled = true;
        private static bool s_includeTimestamp = false;

        /// <summary>
        /// Globally enable or disable all DebugX output.
        /// Applies only in Editor or Development builds.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void SetEnabled(bool enabled) => s_enabled = enabled;

        /// <summary>
        /// Include Time.realtimeSinceStartup in each log prefix.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void SetTimestamp(bool enabled) => s_includeTimestamp = enabled;

        // ---------------------------------------------------------------------
        // Channels (tag-based logging)
        // ---------------------------------------------------------------------
        // You can enable/disable a tag (channel) so only some logs are shown.
        private static readonly Dictionary<string, bool> s_channelEnabled =
            new Dictionary<string, bool>(StringComparer.Ordinal);

        /// <summary>Enable a logging channel (case-sensitive).</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void EnableChannel(string channel) => s_channelEnabled[channel ?? ""] = true;

        /// <summary>Disable a logging channel (case-sensitive).</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void DisableChannel(string channel) => s_channelEnabled[channel ?? ""] = false;

        /// <summary>Set channel enabled/disabled (case-sensitive).</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void SetChannel(string channel, bool enabled) => s_channelEnabled[channel ?? ""] = enabled;

        private static bool IsChannelEnabled(string channel)
        {
            if (string.IsNullOrEmpty(channel))
                return true; // no tag → always allowed

            return !s_channelEnabled.TryGetValue(channel, out var enabled) || enabled;
        }

        // ---------------------------------------------------------------------
        // Color presets
        // ---------------------------------------------------------------------
        public enum PresetColor
        {
            None, Red, Green, Blue, Yellow, Cyan, Magenta, Grey, Orange
        }

        private static string PresetToHex(PresetColor p) => p switch
        {
            PresetColor.Red => "#E74C3C",
            PresetColor.Green => "#2ECC71",
            PresetColor.Blue => "#3498DB",
            PresetColor.Yellow => "#F1C40F",
            PresetColor.Cyan => "#1ABC9C",
            PresetColor.Magenta => "#E91E63",
            PresetColor.Grey => "#95A5A6",
            PresetColor.Orange => "#E67E22",
            _ => null
        };

        // ---------------------------------------------------------------------
        // Core formatting
        // ---------------------------------------------------------------------
        private static string Format(object message, string tag, string hex)
        {
            var text = message?.ToString() ?? "null";

            if (!string.IsNullOrEmpty(tag))
                text = $"[{tag}] {text}";

            if (s_includeTimestamp)
                text = $"[{Time.realtimeSinceStartup:0.000}] {text}";

            if (!string.IsNullOrEmpty(hex))
                text = $"<color={hex}>{text}</color>";

            return text;
        }

        // ---------------------------------------------------------------------
        // Basic logs (Log / Warning / Error) + Unity-like overloads
        // ---------------------------------------------------------------------

        /// <summary>
        /// Standard log with optional tag/context/color.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(
            object message,
            string tag = null,
            UnityEngine.Object context = null,
            PresetColor color = PresetColor.None,
            string hex = null)
        {
            if (!s_enabled || !IsChannelEnabled(tag)) return;
            var h = hex ?? PresetToHex(color);
            UnityEngine.Debug.Log(Format(message, tag, h), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object message)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.Log(Format(message, null, null));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object message, UnityEngine.Object context)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.Log(Format(message, null, null), context);
        }

        /// <summary>Warning log with optional tag/context/color.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(
            object message,
            string tag = null,
            UnityEngine.Object context = null,
            PresetColor color = PresetColor.None,
            string hex = null)
        {
            if (!s_enabled || !IsChannelEnabled(tag)) return;
            var h = hex ?? PresetToHex(color);
            UnityEngine.Debug.LogWarning(Format(message, tag, h), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(object message)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogWarning(Format(message, null, null));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(object message, UnityEngine.Object context)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogWarning(Format(message, null, null), context);
        }

        /// <summary>Error log with optional tag/context/color.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogError(
            object message,
            string tag = null,
            UnityEngine.Object context = null,
            PresetColor color = PresetColor.None,
            string hex = null)
        {
            if (!s_enabled || !IsChannelEnabled(tag)) return;
            var h = hex ?? PresetToHex(color);
            UnityEngine.Debug.LogError(Format(message, tag, h), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogError(object message)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogError(Format(message, null, null));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogError(object message, UnityEngine.Object context)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogError(Format(message, null, null), context);
        }

        /// <summary>Formatted log (string.Format) with tag.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogFormat(string tag, UnityEngine.Object context, string format, params object[] args)
        {
            if (!s_enabled || !IsChannelEnabled(tag)) return;
            UnityEngine.Debug.Log(Format(string.Format(format, args), tag, null), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogFormat(UnityEngine.Object context, string format, params object[] args)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.Log(Format(string.Format(format, args), null, null), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogFormat(string format, params object[] args)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.Log(Format(string.Format(format, args), null, null));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogWarning(Format(string.Format(format, args), null, null), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarningFormat(string format, params object[] args)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogWarning(Format(string.Format(format, args), null, null));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogError(Format(string.Format(format, args), null, null), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogErrorFormat(string format, params object[] args)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogError(Format(string.Format(format, args), null, null));
        }

        /// <summary>Log only if condition is true.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogIf(
            bool condition,
            object message,
            string tag = null,
            UnityEngine.Object context = null,
            PresetColor color = PresetColor.None)
        {
            if (condition)
                Log(message, tag, context, color);
        }

        /// <summary>
        /// Dump an object as JSON using Unity's JsonUtility.
        /// Good for inspecting simple serializable objects.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogJson(object obj, bool pretty = true, string tag = "JSON", UnityEngine.Object context = null)
        {
            if (!s_enabled || !IsChannelEnabled(tag)) return;
            var json = obj == null ? "null" : JsonUtility.ToJson(obj, pretty);
            UnityEngine.Debug.Log(Format(json, tag, PresetToHex(PresetColor.Cyan)), context);
        }

        /// <summary>
        /// Shortcut to log to a specific channel name.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogChannel(string channel, object message, UnityEngine.Object context = null,
                                      PresetColor color = PresetColor.None, string hex = null)
        {
            Log(message, channel, context, color, hex);
        }

        // ---------------------------------------------------------------------
        // Quick perf/memory/FPS
        // ---------------------------------------------------------------------
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogMemory(string tag = "MEM", UnityEngine.Object context = null)
        {
            if (!s_enabled || !IsChannelEnabled(tag)) return;
            long total = Profiler.GetTotalAllocatedMemoryLong();
            long gc = GC.GetTotalMemory(false);
            string msg = $"Memory Alloc: {FormatBytes(total)}, GC Heap: {FormatBytes(gc)}";
            UnityEngine.Debug.Log(Format(msg, tag, PresetToHex(PresetColor.Grey)), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogFPS(string tag = "FPS", UnityEngine.Object context = null)
        {
            if (!s_enabled || !IsChannelEnabled(tag)) return;
            float fps = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.00001f);
            string msg = $"FPS: {fps:0.0}";
            UnityEngine.Debug.Log(Format(msg, tag, PresetToHex(PresetColor.Green)), context);
        }

        private static string FormatBytes(long bytes)
        {
            const double KB = 1024.0;
            const double MB = KB * 1024.0;
            const double GB = MB * 1024.0;
            if (bytes >= GB) return (bytes / GB).ToString("0.00") + " GB";
            if (bytes >= MB) return (bytes / MB).ToString("0.00") + " MB";
            if (bytes >= KB) return (bytes / KB).ToString("0.00") + " KB";
            return bytes + " B";
        }

        // ---------------------------------------------------------------------
        // Exceptions & assertions
        // ---------------------------------------------------------------------
        /// <summary>Logs an exception with Unity's clickable stack trace.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Exception(Exception ex, string tag = null, UnityEngine.Object context = null)
        {
            if (!s_enabled || !IsChannelEnabled(tag)) return;
            UnityEngine.Debug.LogException(ex, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogException(Exception ex)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogException(ex);
        }

        /// <summary>
        /// Simple assert that logs an error instead of throwing.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Assert(
            bool condition,
            string message = "Assertion failed",
            string tag = "ASSERT",
            UnityEngine.Object context = null)
        {
            if (!s_enabled || condition || !IsChannelEnabled(tag)) return;
            UnityEngine.Debug.LogError(Format(message, tag, PresetToHex(PresetColor.Red)), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Assert(bool condition)
        {
            if (!s_enabled) return;
            if (!condition)
                UnityEngine.Debug.LogError(Format("Assertion failed", "ASSERT", PresetToHex(PresetColor.Red)));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Assert(bool condition, object message)
        {
            if (!s_enabled) return;
            if (!condition)
                UnityEngine.Debug.LogError(Format(message, "ASSERT", PresetToHex(PresetColor.Red)));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogAssertion(object message, UnityEngine.Object context = null)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogAssertion(Format(message, "ASSERT", PresetToHex(PresetColor.Red)), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogAssertion(object message)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogAssertion(Format(message, "ASSERT", PresetToHex(PresetColor.Red)));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogAssertionFormat(UnityEngine.Object context, string format, params object[] args)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogAssertion(
                Format(string.Format(format, args), "ASSERT", PresetToHex(PresetColor.Red)),
                context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogAssertionFormat(string format, params object[] args)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.LogAssertion(
                Format(string.Format(format, args), "ASSERT", PresetToHex(PresetColor.Red)));
        }

        /// <summary>
        /// Executes an action and logs any thrown exception.
        /// Helpful to protect small debug-only code.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Try(Action action, string tag = "TRY", UnityEngine.Object context = null)
        {
            if (!s_enabled || !IsChannelEnabled(tag)) return;
            try { action?.Invoke(); }
            catch (Exception ex) { UnityEngine.Debug.LogException(ex, context); }
        }

        /// <summary>
        /// Editor-only break.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Break()
        {
#if UNITY_EDITOR
            if (!s_enabled) return;
            UnityEngine.Debug.Break();
#endif
        }

        // ---------------------------------------------------------------------
        // Log-once / rate-limited / change-tracking
        // ---------------------------------------------------------------------
        private static readonly HashSet<string> s_once =
            new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// Logs a message only once per unique key.
        /// Good for spammy warnings.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogOnce(
            string key,
            object message,
            string tag = null,
            UnityEngine.Object context = null,
            PresetColor color = PresetColor.None,
            string hex = null)
        {
            if (!s_enabled || !IsChannelEnabled(tag)) return;
            if (!s_once.Add(key)) return;
            Log(message, tag, context, color, hex);
        }

        private static readonly Dictionary<string, float> s_lastLogTime =
            new Dictionary<string, float>(StringComparer.Ordinal);

        /// <summary>
        /// Logs at most once per interval per key.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogRate(
            string key,
            float intervalSeconds,
            object message,
            string tag = null,
            UnityEngine.Object context = null,
            PresetColor color = PresetColor.None,
            string hex = null)
        {
            if (!s_enabled || !IsChannelEnabled(tag)) return;
            float now = Time.realtimeSinceStartup;
            if (!s_lastLogTime.TryGetValue(key, out var last) || now - last >= intervalSeconds)
            {
                s_lastLogTime[key] = now;
                Log(message, tag, context, color, hex);
            }
        }

        private static readonly Dictionary<string, string> s_lastValue =
            new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>
        /// Logs when a value for a given key changes.
        /// Good for observing state transitions.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogOnChange<T>(string key, T value, string tag = null, UnityEngine.Object context = null)
        {
            if (!s_enabled || !IsChannelEnabled(tag)) return;
            string s = value?.ToString() ?? "null";
            if (!s_lastValue.TryGetValue(key, out var prev) || prev != s)
            {
                s_lastValue[key] = s;
                Log($"Changed: {key} = {s}", tag, context, PresetColor.Yellow);
            }
        }

        /// <summary>
        /// Clears all guard caches (once/rate/change).
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ResetGuards()
        {
            s_once.Clear();
            s_lastLogTime.Clear();
            s_lastValue.Clear();
        }

        // ---------------------------------------------------------------------
        // Profiler helpers
        // ---------------------------------------------------------------------
        /// <summary>Begins a profiler sample (visible in Unity Profiler).</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void BeginSample(string name) => Profiler.BeginSample(name);

        /// <summary>Ends the last profiler sample.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void EndSample() => Profiler.EndSample();

        /// <summary>
        /// A small RAII timing scope: using (DebugX.Measure("Label")) { ... }
        /// Logs elapsed time on Dispose (Editor/Dev only).
        /// </summary>
        public readonly struct TimeScope : IDisposable
        {
            private readonly string _label;
            private readonly string _tag;
            private readonly UnityEngine.Object _context;
            private readonly float _start;
            private readonly bool _active;

            public TimeScope(string label, string tag = "TIMER", UnityEngine.Object context = null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _label = label;
                _tag = tag;
                _context = context;
                _start = Time.realtimeSinceStartup;
                _active = s_enabled && IsChannelEnabled(tag);
#else
                _label = null;
                _tag = null;
                _context = null;
                _start = 0f;
                _active = false;
#endif
            }

            public void Dispose()
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (!_active) return;
                float ms = (Time.realtimeSinceStartup - _start) * 1000f;
                Log($"{_label} took {ms:0.###} ms", _tag, _context, PresetColor.Grey);
#endif
            }
        }

        /// <summary>
        /// Creates a timing scope for `using (DebugX.Measure("MyTask")) { ... }`
        /// </summary>
        public static TimeScope Measure(string label, string tag = "TIMER", UnityEngine.Object context = null)
            => new TimeScope(label, tag, context);

        // ---------------------------------------------------------------------
        // Drawing helpers (world-space)
        // ---------------------------------------------------------------------
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f, bool depthTest = true)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration = 0f, bool depthTest = true)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void DrawRect2D(Rect rect, Color color, float duration = 0f)
        {
            if (!s_enabled) return;
            var a = new Vector3(rect.xMin, rect.yMin, 0);
            var b = new Vector3(rect.xMax, rect.yMin, 0);
            var c = new Vector3(rect.xMax, rect.yMax, 0);
            var d = new Vector3(rect.xMin, rect.yMax, 0);
            UnityEngine.Debug.DrawLine(a, b, color, duration);
            UnityEngine.Debug.DrawLine(b, c, color, duration);
            UnityEngine.Debug.DrawLine(c, d, color, duration);
            UnityEngine.Debug.DrawLine(d, a, color, duration);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void DrawBounds(Bounds b, Color color, float duration = 0f)
        {
            if (!s_enabled) return;
            var c = b.center;
            var s = b.size;
            var x = new Vector3(s.x / 2f, 0, 0);
            var y = new Vector3(0, s.y / 2f, 0);
            var z = new Vector3(0, 0, s.z / 2f);

            var p000 = c - x - y - z; var p100 = c + x - y - z;
            var p010 = c - x + y - z; var p110 = c + x + y - z;
            var p001 = c - x - y + z; var p101 = c + x - y + z;
            var p011 = c - x + y + z; var p111 = c + x + y + z;

            UnityEngine.Debug.DrawLine(p000, p100, color, duration);
            UnityEngine.Debug.DrawLine(p100, p110, color, duration);
            UnityEngine.Debug.DrawLine(p110, p010, color, duration);
            UnityEngine.Debug.DrawLine(p010, p000, color, duration);

            UnityEngine.Debug.DrawLine(p001, p101, color, duration);
            UnityEngine.Debug.DrawLine(p101, p111, color, duration);
            UnityEngine.Debug.DrawLine(p111, p011, color, duration);
            UnityEngine.Debug.DrawLine(p011, p001, color, duration);

            UnityEngine.Debug.DrawLine(p000, p001, color, duration);
            UnityEngine.Debug.DrawLine(p100, p101, color, duration);
            UnityEngine.Debug.DrawLine(p110, p111, color, duration);
            UnityEngine.Debug.DrawLine(p010, p011, color, duration);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void DrawCircle(
            Vector3 center,
            float radius,
            Color color,
            Vector3 normal = default,
            int segments = 32,
            float duration = 0f)
        {
            if (!s_enabled) return;
            if (segments < 3) segments = 3;
            if (normal == default) normal = Vector3.up;

            // build an orthonormal basis on the plane
            Vector3 u = Vector3.Cross(
                normal,
                Math.Abs(Vector3.Dot(normal, Vector3.right)) > 0.99f
                    ? Vector3.forward
                    : Vector3.right).normalized;
            Vector3 v = Vector3.Cross(normal, u).normalized;

            Vector3 prev = center + radius * u;
            float step = Mathf.PI * 2f / segments;

            for (int i = 1; i <= segments; i++)
            {
                float t = i * step;
                Vector3 next = center + radius * (Mathf.Cos(t) * u + Mathf.Sin(t) * v);
                UnityEngine.Debug.DrawLine(prev, next, color, duration);
                prev = next;
            }
        }

        /// <summary>Draws an arrow from 'from' to 'to'.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void DrawArrow(
            Vector3 from,
            Vector3 to,
            Color color,
            float headLength = 0.25f,
            float headAngle = 20f,
            float duration = 0f)
        {
            if (!s_enabled) return;
            UnityEngine.Debug.DrawLine(from, to, color, duration);

            Vector3 dir = (to - from);
            if (dir.sqrMagnitude < 1e-6f) return;
            dir.Normalize();

            Vector3 right = Quaternion.LookRotation(dir) *
                            Quaternion.Euler(0, 180 + headAngle, 0) *
                            Vector3.forward;
            Vector3 left = Quaternion.LookRotation(dir) *
                           Quaternion.Euler(0, 180 - headAngle, 0) *
                           Vector3.forward;

            UnityEngine.Debug.DrawLine(to, to + right * headLength, color, duration);
            UnityEngine.Debug.DrawLine(to, to + left * headLength, color, duration);
        }
    }
}

// ---------------------------------------------------------------------
// Quick usage (for other developers)
// ---------------------------------------------------------------------
/*
// 1) Basic logging
DebugX.Log("Hello debug");
DebugX.LogWarning("Something looks odd", "AI");
DebugX.LogError("Something failed", "NETWORK");

// 2) Channels
DebugX.EnableChannel("AI");
DebugX.Log("AI thinking...", "AI"); // will show
DebugX.DisableChannel("AI");
DebugX.Log("AI suppressed", "AI");  // will NOT show

// 3) JSON dump
DebugX.LogJson(playerStats);

// 4) Log once
DebugX.LogOnce("missing-config", "Config not found, using default");

// 5) Measure scope
using (DebugX.Measure("HeavyLoop"))
{
    // expensive code here
}

// 6) Drawing
DebugX.DrawLine(Vector3.zero, Vector3.one, Color.green, 2f);
DebugX.DrawBounds(myCollider.bounds, Color.yellow);

// 7) Memory / FPS
DebugX.LogMemory();
DebugX.LogFPS();

// Disclaimer:
// - Calls are compiled out of non-Editor / non-Development builds.
// - Do not depend on this for gameplay logic.
*/
