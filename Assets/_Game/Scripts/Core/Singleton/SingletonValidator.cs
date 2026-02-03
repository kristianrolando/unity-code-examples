#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Singleton
{
    /// <summary>
    /// Validates singleton instances in the scene when entering Play Mode.
    /// Logs an error if multiple instances of the same singleton type are detected.
    /// </summary>
    [InitializeOnLoad]
    public static class SingletonValidator
    {
        static SingletonValidator()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                var allSingletons = Object.FindObjectsByType<BaseSingleton<MonoBehaviour>>(FindObjectsSortMode.None);
                var instanceCountByType = new Dictionary<System.Type, int>();

                foreach (var singleton in allSingletons)
                {
                    var type = singleton.GetType();
                    if (!instanceCountByType.ContainsKey(type))
                        instanceCountByType[type] = 0;

                    instanceCountByType[type]++;
                }

                foreach (var kvp in instanceCountByType)
                {
                    if (kvp.Value > 1)
                        Debug.LogError($"[SingletonValidator] Multiple instances of {kvp.Key.Name} detected in the scene!");
                }
            }
        }
    }
}
#endif
