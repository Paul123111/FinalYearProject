using Mirror;
using System;
using System.Diagnostics;
using UnityEngine;

public class ForceMirrorQuit : MonoBehaviour {

    private void Start() {
        // stops Unity from putting the game thread to sleep when tabbed out
        QualitySettings.vSyncCount = 0;
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
    }

    private void OnApplicationFocus(bool hasFocus) {
        if (!hasFocus) {
            // 1. Force a clean garbage collection pass BEFORE Windows slows down the app thread
            GC.Collect();

            // 2. Clamp target frame rate to prevent the GPU thread from racing ahead of the network thread
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
        } else {
            // 3. Gracefully return to native timing configurations when tabbed back in
            Application.targetFrameRate = 60;
        }
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

