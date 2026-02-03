using System;
using System.Collections;
using UnityEngine;
using Debug = Game.Utilities.DebugX;

namespace Game.Utilities
{
    /// <summary>
    /// Provides reusable coroutine utilities to simplify common animation and timing patterns.
    /// </summary>
    public static class CoroutineHelper
    {
        /// <summary>
        /// Waits for a specified delay and then invokes an action.
        /// </summary>
        public static IEnumerator Wait(float delay, Action onAction = null)
        {
            yield return new WaitForSeconds(delay);
            onAction?.Invoke();
        }

        /// <summary>
        /// Waits while the given predicate returns true.
        /// </summary>
        public static IEnumerator WaitWhile(Func<bool> predicate, Action onComplete = null)
        {
            yield return new WaitWhile(predicate);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Waits until the given predicate returns true.
        /// </summary>
        public static IEnumerator WaitUntil(Func<bool> predicate, Action onComplete = null)
        {
            yield return new WaitUntil(predicate);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Moves a UI element (RectTransform) from one position to another over time.
        /// </summary>
        public static IEnumerator MoveUIRoutine(RectTransform target, Vector2 from, Vector2 to, float speed, Action onComplete = null)
        {
            if (target == null)
            {
                Debug.LogWarning("Target RectTransform is null!");
                yield break;
            }

            float distance = Vector2.Distance(from, to);
            if (distance <= 0.01f)
            {
                target.anchoredPosition = to;
                onComplete?.Invoke();
                yield break;
            }

            target.anchoredPosition = from;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * speed;
                target.anchoredPosition = Vector2.Lerp(from, to, Mathf.Clamp01(t));
                yield return null;
            }

            target.anchoredPosition = to;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Smoothly moves a 3D transform from one position to another.
        /// </summary>
        public static IEnumerator MoveTransformRoutine(Transform target, Vector3 from, Vector3 to, float speed, Action onComplete = null)
        {
            if (target == null) yield break;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * speed;
                target.position = Vector3.Lerp(from, to, Mathf.Clamp01(t));
                yield return null;
            }

            target.position = to;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Fades a CanvasGroup in or out.
        /// </summary>
        public static IEnumerator FadeCanvasGroup(CanvasGroup canvas, float from, float to, float duration, Action onComplete = null)
        {
            if (canvas == null) yield break;

            float elapsed = 0f;
            canvas.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvas.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            canvas.alpha = to;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Scales a transform over time.
        /// </summary>
        public static IEnumerator ScaleRoutine(Transform target, Vector3 from, Vector3 to, float duration, Action onComplete = null)
        {
            if (target == null) yield break;

            float elapsed = 0f;
            target.localScale = from;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                target.localScale = Vector3.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            target.localScale = to;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Repeats an action every interval for a total duration.
        /// </summary>
        public static IEnumerator InvokeRepeating(Action action, float interval, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                action?.Invoke();
                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }
        }

        /// <summary>
        /// Creates a ping-pong animation between two values for a duration.
        /// </summary>
        public static IEnumerator PingPongFloat(float from, float to, float duration, Action<float> onValueChanged)
        {
            float t = 0f;
            while (t < duration)
            {
                float value = Mathf.Lerp(from, to, Mathf.PingPong(Time.time, 1f));
                onValueChanged?.Invoke(value);
                yield return null;
                t += Time.deltaTime;
            }
        }
    }
}
