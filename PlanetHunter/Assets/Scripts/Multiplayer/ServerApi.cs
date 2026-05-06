using MiniJSON;
using Newtonsoft.Json;
using System;
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
    [SerializeField] GameObject roomUI;
    [SerializeField] RectTransform roomsPanel;

    public async Task<string> ListRooms() {
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

    async void Start() {
        ButtonListRooms();
    }

    public async void ButtonListRooms() {
        refreshButton.interactable = false;
        Debug.Log("Calling API...");
        string jsonResponse = await ListRooms();
        GameServerResponse[] serversJson = JsonHelper.ParseArray<GameServerResponse>(jsonResponse);
        Debug.Log(serversJson);
        foreach (Transform child in roomsPanel) {
            Destroy(child.gameObject);
        }
        foreach (GameServerResponse server in serversJson) {
            for (int i = 0; i < 3; i++) {
                CreateRoomUI(server);
            }
        }
        refreshButton.interactable = true;
    }

    public async void ButtonAllocate() {
        refreshButton.interactable = false;
        Debug.Log("Calling API...");
        string jsonResponse = await Allocate();
        refreshButton.interactable = true;
    }

    void CreateRoomUI(GameServerResponse json) {
        GameObject room = Instantiate(roomUI, roomsPanel);
        RoomItemUI roomItemUI = room.GetComponent<RoomItemUI>();
        roomItemUI.SetServerJson(json);
    }
}
