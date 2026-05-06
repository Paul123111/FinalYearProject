using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;

// calls the api server endpoints

public class ServerApi : MonoBehaviour
{
    [SerializeField] string baseUrl = "http://192.168.1.19:30001";
    [SerializeField] Button refreshButton;

    public async Task ListRooms() {
        string token = AuthenticationService.Instance.AccessToken;
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/listrooms")) {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Accept", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success) {
                Debug.Log("Response: " + request.downloadHandler.text);
            } else {
                Debug.LogError($"Error {request.responseCode}: {request.error}");
            }
        }
    }

    public async void ButtonListRooms() {
        refreshButton.interactable = false;
        Debug.Log("Calling API...");
        await ListRooms();
        refreshButton.interactable = true;
    }
}
