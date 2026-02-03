using System.Collections;
using UnityEngine;

namespace Game.Utilities
{
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner m_instance;

        public static Coroutine Start(IEnumerator routine)
        {
            if (m_instance == null)
            {
                GameObject runnerObj = new GameObject("[CoroutineRunner]");
                m_instance = runnerObj.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(runnerObj);
            }

            return m_instance.StartCoroutine(routine);
        }

        public static void Stop(Coroutine coroutine)
        {
            if (m_instance != null && coroutine != null)
            {
                m_instance.StopCoroutine(coroutine);
            }
        }

        public static void StopAll()
        {
            if (m_instance != null)
            {
                m_instance.StopAllCoroutines();
            }
        }
    }
}
