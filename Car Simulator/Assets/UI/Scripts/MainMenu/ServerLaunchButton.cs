using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ServerLaunchButton : MonoBehaviour
{
    public void LaunchServer()
    {
        string serverPath = Path.GetFullPath(Path.Combine(Application.dataPath, @"..\..\server"));
        System.Diagnostics.Process.Start(serverPath + @"\launch_server.bat");
    }
}
