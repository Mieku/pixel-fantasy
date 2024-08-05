using System;
using System.Collections;
using System.Globalization;
using System.IO;
using Systems.Game_Setup.Scripts;
using UnityEngine;

namespace Controllers
{
    public class ScreenshotController : MonoBehaviour
    {
        public int ScreenshotWidth = 1920;
        public int ScreenshotHeight = 1080;

        private Camera _screenshotCam;
        private static string SaveScreenshotPath;
        private static string NormalScreenshotPath;

        private void Awake()
        {
            _screenshotCam = GetComponent<Camera>();
            
            SaveScreenshotPath = $"{Application.persistentDataPath}/Saves/SaveScreenshots";
            NormalScreenshotPath = $"{Application.persistentDataPath}/Screenshots";
        }
        
        public void TakeScreenshot()
        {
            if (!Directory.Exists(NormalScreenshotPath))
            {
                Directory.CreateDirectory(NormalScreenshotPath);
            }

            string fileName = $"{GameManager.Instance?.GameData?.SettlementName ?? "Screenshot"}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)}";
            fileName = SanitizeFileName(fileName);
            string filePath = Path.Combine(NormalScreenshotPath, fileName + ".png");
            StartCoroutine(TakeScreenshotCoroutine(filePath));
        }
        
        public string TakeSaveScreenshot(string fileName)
        {
            if (!Directory.Exists(SaveScreenshotPath))
            {
                Directory.CreateDirectory(SaveScreenshotPath);
            }
            
            fileName = SanitizeFileName(fileName);
            string filePath = Path.Combine(SaveScreenshotPath, fileName + ".png");
            StartCoroutine(TakeScreenshotCoroutine(filePath));

            return filePath;
        }
        
        private string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_'); // Replace invalid characters with an underscore
            }
            return fileName;
        }
        
        private IEnumerator TakeScreenshotCoroutine(string filePath)
        {
            // Create a temporary RenderTexture
            RenderTexture renderTexture = new RenderTexture(ScreenshotWidth, ScreenshotHeight, 24);
            _screenshotCam.targetTexture = renderTexture;

            // Enable ScreenshotCamera for rendering
            _screenshotCam.enabled = true;

            // Wait until the end of the frame
            yield return new WaitForEndOfFrame();

            // Render the camera's view to the RenderTexture
            _screenshotCam.Render();

            // Create a Texture2D to save the RenderTexture
            Texture2D screenshot = new Texture2D(ScreenshotWidth, ScreenshotHeight, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            screenshot.ReadPixels(new Rect(0, 0, ScreenshotWidth, ScreenshotHeight), 0, 0);
            screenshot.Apply();

            // Save the screenshot to a file
            byte[] bytes = screenshot.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);

            // Clean up
            _screenshotCam.targetTexture = null;
            RenderTexture.active = null;
            Destroy(renderTexture);
            Destroy(screenshot);

            // Disable ScreenshotCamera after capturing
            _screenshotCam.enabled = false;

            yield return null;
        }
    }
}
