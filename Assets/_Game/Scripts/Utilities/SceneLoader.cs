using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Game.Utilities
{
    public static class SceneLoader
    {
        private class SceneLoaderBehaviour : MonoBehaviour { }

        private static SceneLoaderBehaviour loaderHelper;

        private static void EnsureHelperExists()
        {
            if (loaderHelper == null)
            {
                GameObject obj = new GameObject("[SceneLoaderHelper]");
                UnityEngine.Object.DontDestroyOnLoad(obj);
                loaderHelper = obj.AddComponent<SceneLoaderBehaviour>();
            }
        }

        /// <summary>
        /// Loads a scene asynchronously with optional callbacks and additive mode.
        /// </summary>
        public static void LoadSceneAsync(string sceneName, Action<float> onProgress = null, Action onCompleted = null, bool additive = false)
        {
            EnsureHelperExists();
            loaderHelper.StartCoroutine(LoadRoutine(sceneName, onProgress, onCompleted, additive));
        }

        private static IEnumerator LoadRoutine(string sceneName, Action<float> onProgress, Action onCompleted, bool additive)
        {
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            asyncOp.allowSceneActivation = false;

            while (asyncOp.progress < 0.9f)
            {
                onProgress?.Invoke(asyncOp.progress);
                yield return null;
            }

            // Finish progress
            onProgress?.Invoke(1f);

            // Activate scene
            asyncOp.allowSceneActivation = true;

            // Wait until done
            while (!asyncOp.isDone)
                yield return null;

            onCompleted?.Invoke();
        }

        /// <summary>
        /// Unloads an additive scene asynchronously.
        /// </summary>
        public static void UnloadSceneAsync(string sceneName, Action onCompleted = null)
        {
            EnsureHelperExists();
            loaderHelper.StartCoroutine(UnloadRoutine(sceneName, onCompleted));
        }

        private static IEnumerator UnloadRoutine(string sceneName, Action onCompleted)
        {
            AsyncOperation asyncOp = SceneManager.UnloadSceneAsync(sceneName);

            while (!asyncOp.isDone)
                yield return null;

            onCompleted?.Invoke();
        }

        /// <summary>
        /// Reloads the currently active scene.
        /// </summary>
        public static void ReloadCurrentScene(Action<float> onProgress = null, Action onCompleted = null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            LoadSceneAsync(currentScene, onProgress, onCompleted);
        }
    }
}
