using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Debug = Game.Utilities.DebugX;

namespace Game.Utilities
{
    /// <summary>
    /// Utility for capturing screenshots and recording gameplay frames.
    /// Useful for debugging, documentation, or creating promotional content.
    /// </summary>
    public static class ScreenshotTool
    {
        private const string ScreenshotFolder = "Screenshots";
        private const string RecordingFolder = "RecordingFrames";

        public enum ImageFormat { PNG, JPG }

        private static MonoBehaviour _runner;
        private static Coroutine _recordCoroutine;
        private static int _frameCount;
        private static bool _isRecording;

        /// <summary>
        /// Initializes the ScreenshotTool with a MonoBehaviour to run coroutines.
        /// Typically called from GameManager or any persistent MonoBehaviour.
        /// </summary>
        public static void Init(MonoBehaviour runner)
        {
            _runner = runner;
        }

        /// <summary>
        /// Captures a full screen screenshot.
        /// </summary>
        /// <param name="prefix">Filename prefix.</param>
        /// <param name="format">Image format (PNG/JPG).</param>
        /// <param name="superSize">Resolution multiplier (e.g., 2 = 2x screen size).</param>
        public static void TakeScreenshot(string prefix = "Screenshot", ImageFormat format = ImageFormat.PNG, int superSize = 1)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, ScreenshotFolder);
            Directory.CreateDirectory(folderPath);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string extension = format == ImageFormat.PNG ? "png" : "jpg";
            string filename = $"{prefix}_{timestamp}.{extension}";
            string path = Path.Combine(folderPath, filename);

            superSize = Mathf.Clamp(superSize, 1, 5); // Protect against huge values
            ScreenCapture.CaptureScreenshot(path, superSize);

#if UNITY_EDITOR
            Debug.Log($"📸 Screenshot saved to: {path}");
#endif
        }

        /// <summary>
        /// Captures the output of a specific camera to an image file.
        /// </summary>
        public static void TakeCameraScreenshot(Camera cam, int width, int height, string prefix = "CamShot", ImageFormat format = ImageFormat.PNG)
        {
            RenderTexture rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;

            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.Destroy(rt);

            string folderPath = Path.Combine(Application.persistentDataPath, ScreenshotFolder);
            Directory.CreateDirectory(folderPath);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string extension = format == ImageFormat.PNG ? "png" : "jpg";
            string filename = $"{prefix}_{timestamp}.{extension}";
            string path = Path.Combine(folderPath, filename);

            byte[] bytes = format == ImageFormat.PNG
                ? screenShot.EncodeToPNG()
                : screenShot.EncodeToJPG();

            File.WriteAllBytes(path, bytes);
#if UNITY_EDITOR
            Debug.Log($"📷 Camera screenshot saved to: {path}");
#endif
        }

        #region 🎥 Frame Sequence Recorder

        /// <summary>
        /// Begins recording frames as images. Useful for creating GIFs or video sequences externally.
        /// </summary>
        /// <param name="durationSeconds">Total recording duration.</param>
        /// <param name="frameRate">Frame capture rate (frames per second).</param>
        public static void StartRecording(float durationSeconds, float frameRate = 10f)
        {
            if (_runner == null)
            {
                Debug.LogError("ScreenshotTool.Init() must be called before starting recording.");
                return;
            }

            if (_isRecording)
            {
                Debug.LogWarning("Recording already in progress.");
                return;
            }

            _isRecording = true;
            _frameCount = 0;

            float interval = 1f / frameRate;
            _recordCoroutine = _runner.StartCoroutine(RecordFrames(durationSeconds, interval));
        }

        /// <summary>
        /// Stops recording early if needed.
        /// </summary>
        public static void StopRecording()
        {
            if (!_isRecording || _recordCoroutine == null) return;

            _runner.StopCoroutine(_recordCoroutine);
            _recordCoroutine = null;
            _isRecording = false;

            Debug.Log("📼 Recording manually stopped.");
        }

        private static IEnumerator RecordFrames(float duration, float interval)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, RecordingFolder);
            Directory.CreateDirectory(folderPath);

            float timer = 0f;

            while (timer < duration)
            {
                yield return new WaitForEndOfFrame();

                Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
                string filename = $"Frame_{_frameCount:D4}.png";
                string path = Path.Combine(folderPath, filename);

                File.WriteAllBytes(path, tex.EncodeToPNG());
                UnityEngine.Object.Destroy(tex);

                _frameCount++;
                timer += interval;

                yield return new WaitForSeconds(interval);
            }

            _isRecording = false;
            Debug.Log($"🎞️ Recording finished. Frames saved: {_frameCount}");
        }

        #endregion
    }
}
