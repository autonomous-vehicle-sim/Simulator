using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ScreenshotScript : MonoBehaviour
{
    public string screenshotDirectory = "./screenshots";
    public string screenshotName = "mapScreenshot";
    public string latestScreenshotPath;
    private const int MAP_LAYER = 1;
    private const int TEXTURE_WIDTH = 512;
    private const int TEXTURE_HEIGHT = 512;

    void Start()
    {
        if (!Directory.Exists(screenshotDirectory))
        {
            Directory.CreateDirectory(screenshotDirectory);
        }
    }

    void Update()
    {

    }

    public void TakeScreenshot(Camera camera)
    {
        if (camera != null)
        {
            int originalCullingMask = camera.cullingMask;
            RenderTexture renderTexture = new RenderTexture(TEXTURE_WIDTH, TEXTURE_HEIGHT, 24);
            camera.targetTexture = renderTexture;
            camera.cullingMask = MAP_LAYER;
            camera.Render();
            RenderTexture.active = renderTexture;

            Texture2D image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false, true);
            image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            image.Apply();

            byte[] bytes = image.EncodeToPNG();
            string timestamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            string filePath = Path.Combine(screenshotDirectory, screenshotName + "_" + timestamp + ".png");
            File.WriteAllBytes(filePath, bytes);
            Debug.Log("Screenshot taken and saved to: " + filePath);
            latestScreenshotPath = filePath;
            camera.cullingMask = originalCullingMask;
            camera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(image);
            Destroy(renderTexture);
        }
        else
        {
            Debug.LogError("Screenshot camera not assigned.");
        }
    }
}