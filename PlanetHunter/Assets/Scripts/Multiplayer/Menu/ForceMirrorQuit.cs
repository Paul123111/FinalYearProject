using Mirror;
using System.Diagnostics;
using UnityEngine;

public class ForceMirrorQuit : MonoBehaviour {

    private void Start() {
        // stops Unity from putting the game thread to sleep when tabbed out
        QualitySettings.vSyncCount = 0;
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
    }

    private void OnApplicationQuit() {
        UnityEngine.Debug.Log("Application quitting: Shutting down network lines...");

        // cleanly disconnect the Mirror client from the server
        if (NetworkClient.active) {
            NetworkClient.Disconnect();
        }

        // shut down the local transport layer socket completely
        if (Transport.active != null) {
            Transport.active.Shutdown();
        }
    }
}

