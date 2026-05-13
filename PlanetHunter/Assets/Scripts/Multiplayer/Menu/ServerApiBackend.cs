using kcp2k;
using Mirror;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// calls the api server endpoints and authorises player
// TODO make apiServerBackend script auth if not logged in
// TOOD move ui logic out of ServerApi

public class ServerApiBackend : MonoBehaviour {
    [SerializeField] string baseUrl = "http://192.168.1.19:30001";
    bool auth = false;

    // anonymously signs in a player using Unity Player Services
    async Task UnityAuth() {
        if (!auth) {
            try {
                Debug.Log("Auth Start");

                string profileName = GetCommandLineArg("-profile") ?? "DefaultPlayer";
                var options = new InitializationOptions();
                options.SetProfile(profileName);
                await UnityServices.InitializeAsync(options);

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in anonymously, id is " + AuthenticationService.Instance.PlayerId);
                auth = true;
            } catch(Exception ex) {
                Debug.LogException(ex);
            }
        } else {
            Debug.Log("Already authorised!");
        }
    }

    //----------------------
    //  Endpoints
    //----------------------
    public async Task<string> ListRooms() {
        await UnityAuth();
        string token = AuthenticationService.Instance.AccessToken;
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/listrooms")) {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Accept", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success) {
                Debug.Log("Response: " + request.downloadHandler.text);
                return request.downloadHandler.text;
            } else {
                Debug.LogError($"Error {request.responseCode}: {request.error}");
                return "[]";
            }
        }
    }

    public async Task<string> Allocate() {
        await UnityAuth();
        string token = AuthenticationService.Instance.AccessToken;
        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/allocate", "POST")) {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Accept", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success) {
                Debug.Log("Response: " + request.downloadHandler.text);
                return request.downloadHandler.text;
            } else {
                Debug.LogError($"Error {request.responseCode}: {request.error}");
                return "[]";
            }
        }
    }

    public async Task<bool> GetServer(string ip, int port) {
        await UnityAuth();
        string token = AuthenticationService.Instance.AccessToken;
        string server = ip + ":" + port;
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/get/{server}")) {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Accept", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success) {
                Debug.Log("Response: " + request.downloadHandler.text);
                return bool.Parse(request.downloadHandler.text);
            } else {
                Debug.LogError($"Error {request.responseCode}: {request.error}");
                return false;
            }
        }
    }

    //----------------------
    //  Helper Methods
    //----------------------
    // helper function for testing different unity clients on one machine
    private string GetCommandLineArg(string name) {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == name && args.Length > i + 1) {
                return args[i + 1];
            }
        }
        return null;
    }
}
