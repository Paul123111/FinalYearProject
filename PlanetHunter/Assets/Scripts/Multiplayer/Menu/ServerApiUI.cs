using kcp2k;
using Mirror;
using Mirror.Examples.MultipleMatch;
using UnityEngine;
using UnityEngine.UI;

public class ServerApiUI : MonoBehaviour
{
    Button[] buttons;
    ServerApiBackend serverApi;
    [SerializeField] GameObject roomUi;
    [SerializeField] RectTransform roomsPanel;

    void Start() {
        serverApi = GetComponent<ServerApiBackend>();
    }

    //--------------------
    // Button Methods
    //--------------------
    public async void ListRooms() {
        DisableAllButtons();
        Debug.Log("Calling API...");
        string jsonResponse = await serverApi.ListRooms();
        GameServerResponse[] serversJson = JsonHelper.ParseArray<GameServerResponse>(jsonResponse);
        foreach (Transform child in roomsPanel) {
            Destroy(child.gameObject);
        }
        foreach (GameServerResponse server in serversJson) {
            CreateRoomUI(server);
        }
        EnableAllButtons();
    }

    public async void Allocate() {
        DisableAllButtons();
        Debug.Log("Calling API...");
        string jsonResponse = await serverApi.Allocate();
        GameServerResponse serversJson = JsonUtility.FromJson<GameServerResponse>(jsonResponse);
        if (serversJson.ip != "0.0.0.0" && serversJson.port != 0) {
            Debug.Log("Found server! Connecting...");
            NetworkManager.singleton.networkAddress = serversJson.ip;
            NetworkManager.singleton.GetComponent<KcpTransport>().Port = (ushort)serversJson.port;
            NetworkManager.singleton.StartClient();
        }
        EnableAllButtons();
    }

    public async void JoinServer(GameServerResponse serverJson) {
        DisableAllButtons();

        Debug.Log("Calling API...");
        bool exists = await serverApi.GetServer(serverJson.ip, serverJson.port);
        if (!exists) {
            Debug.LogWarning("Server doesn't exist!");
            return;
        }
        if (serverJson.ip != "0.0.0.0" && serverJson.port != 0) {
            Debug.Log($"Server found! Joining {serverJson.ip}:{serverJson.port}...");
            NetworkManager.singleton.networkAddress = serverJson.ip;
            NetworkManager.singleton.GetComponent<KcpTransport>().Port = (ushort)serverJson.port;
            NetworkManager.singleton.StartClient();
        }

        EnableAllButtons();
    }

    public void SinglePlayer() {
        DisableAllButtons();
        NetworkManager.singleton.StartHost();
        EnableAllButtons();
    }

    //--------------------
    // Helper Methods
    //--------------------
    void DisableAllButtons() {
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var button in buttons) {
            button.interactable = false;
        }
    }

    void EnableAllButtons() {
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var button in buttons) {
            button.interactable = true;
        }
    }

    void CreateRoomUI(GameServerResponse json) {
        GameObject room = Instantiate(roomUi, roomsPanel);
        RoomItemUI roomItemUi = room.GetComponent<RoomItemUI>();
        roomItemUi.SetServerJson(json);
    }
}
