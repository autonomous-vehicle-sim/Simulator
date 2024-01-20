using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;

public class CameraRecorder : MonoBehaviour
{
    [SerializeField] private Camera[] _cameras;
    [SerializeField] private String _capturePath = "C:/UnitySimulator/";
    [SerializeField] private int _screenshotWidth = 256, _screenshotHeight = 256;
    [SerializeField] private float _framesPerSecond = 1;
    [SerializeField] private GameObject _car;

    private int _framesCaptured = 0;
    private float _timeSinceLastCapture = 0;
    private RenderTexture _renderTexture;
    private String _pathTimestamp;
    private WheelController _wheelController;
    private String _dataPath;

    // Start is called before the first frame update
    private void Start()
    {
        _renderTexture = new RenderTexture(_screenshotWidth, _screenshotHeight, 16, RenderTextureFormat.ARGB32);
        _pathTimestamp = DateTime.Now.ToString();
        // Replace ':" with '." as windows directories can't contain ':' in their filepaths
        _pathTimestamp = Regex.Replace(_pathTimestamp, ":", ".");
        _wheelController = _car.GetComponent<WheelController>();
        _dataPath = _capturePath + "/" + _pathTimestamp + ".csv";
        List<String> carParams = new List<String> { "SteeringAngle", "MotorTorque" };
        WritetoCsv(_dataPath, string.Join(";",carParams));
    }
    private void WritetoCsv(String path, String data)
    {
        using (StreamWriter writer = new StreamWriter(path, true))
        {
            writer.WriteLine(string.Join(";", data));
        }
    }
    private void SaveCarData(String path)
    {
        List <float> carData = new List<float>();
        
        carData.Add(_wheelController.CurrentSteeringAngle);
        carData.Add(_wheelController.CurrentMotorTorque);
        WritetoCsv(path, string.Join(";", carData));
    }
    private void SaveScreenshot(String path, Camera camera)
    {
        Texture2D image = RTImage(camera);
        byte[] bytes = image.EncodeToPNG();
        System.IO.FileInfo screenshot = new System.IO.FileInfo(path);
        screenshot.Directory.Create();
        System.IO.File.WriteAllBytes(path, bytes);
    }

    private Texture2D RTImage(Camera camera)
    {
        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        camera.targetTexture = _renderTexture;
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
    private void Update()
    {
        _timeSinceLastCapture += Time.deltaTime;
        if(_timeSinceLastCapture > 1 / _framesPerSecond)
        {
            _timeSinceLastCapture = 0;
            for(int i = 0; i < _cameras.Length; i++)
            {
                String filepath = _capturePath + "/" + _pathTimestamp + "/" + _cameras[i].name + "/" + _framesCaptured.ToString() + ".png";
                SaveScreenshot(filepath, _cameras[i]);
            }
            SaveCarData(_dataPath);
            _framesCaptured++;
        }
    }
}
