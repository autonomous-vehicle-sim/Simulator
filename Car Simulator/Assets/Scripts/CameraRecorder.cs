using UnityEngine;
using System.Collections;
using System;
using System.Text.RegularExpressions;

public class CameraRecorder : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;
    [SerializeField] private String capturePath = "C:/UnitySimulator/";
    [SerializeField] private int screenshotWidth = 256, screenshotHeight = 256;
    [SerializeField] private float framesPerSecond = 1;

    private int framesCaptured = 0;
    private float timeSinceLastCapture = 0;
    private RenderTexture renderTexture;
    private String pathTimestamp;
    // Start is called before the first frame update
    void Start()
    {
        renderTexture = new RenderTexture(screenshotWidth, screenshotHeight, 16, RenderTextureFormat.ARGB32);
        pathTimestamp = DateTime.Now.ToString();
        // Replace ':" with '." as windows directories can't contain ':' in their filepaths
        pathTimestamp = Regex.Replace(pathTimestamp, ":", "."); 
    }

    void SaveScreenshot(String path, Camera camera)
    {
        Texture2D image = RTImage(camera);
        byte[] bytes = image.EncodeToPNG();
        System.IO.FileInfo screenshot = new System.IO.FileInfo(path);
        screenshot.Directory.Create();
        System.IO.File.WriteAllBytes(path, bytes);
    }

    Texture2D RTImage(Camera camera)
    {
        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        camera.targetTexture = renderTexture;
        RenderTexture.active = camera.targetTexture;

        // Render the camera's view.
        camera.Render();

        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        image.Apply();

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;
        return image;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastCapture += Time.deltaTime;
        if(timeSinceLastCapture > 1 / framesPerSecond)
        {
            timeSinceLastCapture = 0;
            for(int i = 0; i < cameras.Length; i++)
            {
                String filepath = capturePath + "/" + pathTimestamp + "/" + cameras[i].name + "/" + framesCaptured.ToString() + ".png";
                SaveScreenshot(filepath, cameras[i]);
            }
            framesCaptured++;
        }
    }
}
