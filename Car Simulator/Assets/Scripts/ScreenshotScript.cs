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
    public string screenshotDirectory = "C:\\UnitySimulator";
    public string screenshotName = "mapScreenshot";
    private const int MAP_LAYER = 1 << 0;
    private const int TEXTURE_WIDTH = 1920;
    private const int TEXTURE_HEIGHT = 1080;

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
 
    public IEnumerator TakeScreenshot(Camera cameras)
    {
        yield return new WaitForEndOfFrame();
        if (cameras != null)
        {

            int originalCullingMask = cameras.cullingMask;
            RenderTexture renderTexture = new RenderTexture(TEXTURE_WIDTH, TEXTURE_HEIGHT, 24);
            cameras.targetTexture = renderTexture;
            cameras.cullingMask = MAP_LAYER;
            cameras.Render();
            RenderTexture.active = renderTexture;

            Texture2D image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false,true);
            image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            image.Apply();

            byte[] bytes = image.EncodeToPNG();
            string timestamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            string filePath = Path.Combine(screenshotDirectory, screenshotName + "_" + timestamp + ".png");
            File.WriteAllBytes(filePath, bytes);
            Debug.Log("Screenshot taken and saved to: " + filePath);

            cameras.cullingMask = originalCullingMask;
            cameras.targetTexture = null;
            RenderTexture.active = null;
            Destroy(image);
            Destroy(renderTexture);
        }
        else
        {
            Debug.LogError("Screenshot camera or render texture not assigned.");
        }
    }
}
