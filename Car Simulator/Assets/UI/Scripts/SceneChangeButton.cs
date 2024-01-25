using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeButton : MonoBehaviour
{
    [SerializeField] private SceneAsset _scene;

    public void ChangeScene()
    {
        Debug.Log("xd");
        if (_scene != null)
        {
#if UNITY_EDITOR
            // EditorSceneManager.SaveOpenScenes();             // not available in play mode
            EditorSceneManager.LoadSceneInPlayMode(AssetDatabase.GetAssetPath(_scene.GetInstanceID()), new(LoadSceneMode.Single, LocalPhysicsMode.Physics3D));
#else
            SceneManager.LoadScene(_scene.GetInstanceID());
#endif
        }
    }
}
