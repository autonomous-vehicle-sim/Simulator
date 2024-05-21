using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLaunchButton : MonoBehaviour
{
    public void LaunchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
