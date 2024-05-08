using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Rendering.LookDev;
using UnityEditor.Rendering;

public class CameraRecorder : MonoBehaviour
{
    [SerializeField] private Camera[] _cameras;
    [SerializeField] private string _capturePath = "C:/UnitySimulator/";
    [SerializeField] private int _screenshotWidth = 256, _screenshotHeight = 256;
    [SerializeField] private float _framesPerSecond = 1.0f;
    [SerializeField] private GameObject _car;

    private int _framesCaptured = 0;
    private float _timeSinceLastCapture = 0;
    private RenderTexture _renderTexture;
    private String _pathTimestamp;
    private CarController _carController;
    private String _dataPath;
    private String _photoPath;
    private const int CAMERA_LAYER = 1;
    public static CameraRecorder Instance;

    // Start is called before the first frame update
    private void Start()
    {
        _renderTexture = new RenderTexture(_screenshotWidth, _screenshotHeight, 16, RenderTextureFormat.ARGB32);
        _pathTimestamp = DateTime.Now.ToString();
        // Replace ':" with '." as windows directories can't contain ':' in their filepaths
        _pathTimestamp = Regex.Replace(_pathTimestamp, ":", ".");
        _carController = _car.GetComponent<CarController>();
        _dataPath = _capturePath + "/" + _pathTimestamp + ".csv";
        _photoPath = _capturePath;
        foreach (Camera _camera in _cameras)
        {
            _camera.cullingMask = CAMERA_LAYER;
        }

        List<String> carParams = new List<String> { "SteeringAngle", "MotorTorque" };
        WritetoCsv(_dataPath, string.Join(";",carParams));
    }
    public void SetPath(string pathTimestamp, string map_id, string car_id)
    {
        _capturePath = _capturePath + pathTimestamp + '/' + map_id + "/" + car_id + '/';
        _dataPath = _capturePath + "carData.csv";
        System.IO.FileInfo directoryPath = new System.IO.FileInfo(_capturePath);
        directoryPath.Directory.Create();
    }
    public void SetCar(GameObject car)
    {
        _car = car;
    }
    public Camera[] GetCameras()
    {
        return _cameras;
    }
    public void SetCameras(Camera[] cameras)
    {
        _cameras = cameras;
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
        
        carData.Add(_carController.CurrentSteeringAngle);
        carData.Add(_carController.CurrentSpeed);
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
        _renderTexture = new RenderTexture(_screenshotWidth, _screenshotHeight, 16, RenderTextureFormat.ARGB32);

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
    public void SavePhoto(String path, string photo_id)
    {
        for (int i = 0; i < _cameras.Length; i++)
        {
            _photoPath = _capturePath;

            String filepath = _photoPath + path + "/" + _cameras[i].name + "/" + photo_id.ToString() + ".png";
            Debug.Log(filepath);
            SaveScreenshot(filepath, _cameras[i]);
        }
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
                String filepath = _capturePath + _cameras[i].name + "/" + _framesCaptured.ToString() + ".png";
                SaveScreenshot(filepath, _cameras[i]);
            }
            _framesCaptured++;
            SaveCarData(_dataPath);
         }
    }
}
