using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;

public class CameraRecorder : MonoBehaviour
{
    [SerializeField] private Camera[] _cameras;
    [SerializeField] private string _capturePath = "C:/UnitySimulator/";
    [SerializeField] private int _screenshotWidth = 256, _screenshotHeight = 256;
    [SerializeField] private float _framesPerSecond = 1.0f;

    private int _framesCaptured = 0;
    private float _timeSinceLastCapture = 0;
    private RenderTexture _renderTexture;
    private String _pathTimestamp;
    private CarController _carController;
    private String _dataPath;
    private TCPClient _client;
    private const int CAMERA_LAYER = 1;
    public static CameraRecorder Instance;
    private float _currentFrameTimestamp;


    // Start is called before the first frame update
    private void Start()
    {
        _renderTexture = new RenderTexture(_screenshotWidth, _screenshotHeight, 16, RenderTextureFormat.ARGB32);
        _pathTimestamp = DateTime.Now.ToString();
        // Replace ':" with '." as windows directories can't contain ':' in their filepaths
        _pathTimestamp = Regex.Replace(_pathTimestamp, ":", ".");
        _carController = gameObject.GetComponent<CarController>();
        _dataPath = _capturePath + "/" + _pathTimestamp + ".csv";
        foreach (Camera _camera in _cameras)
        {
            _camera.cullingMask = CAMERA_LAYER;
        }
        SetPath();
        List<String> carParams = new List<String> { "SteeringAngle", "MotorTorque" };
        WritetoCsv(_dataPath, string.Join(";",carParams));
        SetClient();

    }
    public void SetPath()
    {
        string mapId = _carController.mapId.ToString();
        string carId = _carController.carId.ToString();
        _capturePath = _capturePath + _pathTimestamp + '/' + mapId + "/" + carId + '/';
        _dataPath = _capturePath + "carData.csv";
        System.IO.FileInfo directoryPath = new System.IO.FileInfo(_capturePath);
        directoryPath.Directory.Create();
    }
    

    public void SetClient()
    {
        _client = GameObject.Find("Client").GetComponent<TCPClient>();
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
 
    // Update is called once per frame
    private void Update()
    {
        _timeSinceLastCapture += Time.deltaTime;
        _currentFrameTimestamp = Time.time;
        if (_timeSinceLastCapture > 1 / _framesPerSecond)
        {
            string newPhotosUpdateInfo = _carController.mapId.ToString() + ";" + _carController.carId.ToString() + ";"
                                       + _framesCaptured.ToString() + ";" + _currentFrameTimestamp.ToString();
            _timeSinceLastCapture = 0;
            for(int i = 0; i < _cameras.Length; i++)
            {
                String filepath = _capturePath + _cameras[i].name + "/" + _framesCaptured.ToString() + ".png";
                newPhotosUpdateInfo += ";" + filepath;
                SaveScreenshot(filepath, _cameras[i]);
            }
            _framesCaptured++;
            SaveCarData(_dataPath);
            if(_client != null)
            {
                float steer = _carController.CurrentSteeringAngle;
                float engine = _carController.GetCurrentEngine();
                float carId = _carController.carId;
                float mapId = _carController.mapId;
                string newSteerUpdateInfo = "steer " + mapId + " " + carId + " " + steer.ToString() + " " + _currentFrameTimestamp.ToString();
                string newEngineUpdateInfo = "engine " + mapId + " " + carId + " " + engine.ToString() + " " + _currentFrameTimestamp.ToString();
                _client.SendMessageToServer("screen;" + newPhotosUpdateInfo);
                _client.SendMessageToServer(newSteerUpdateInfo);
                _client.SendMessageToServer(newEngineUpdateInfo);
            }
         }
    }
}
