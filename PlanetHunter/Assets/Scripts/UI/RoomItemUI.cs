using kcp2k;
using Mirror;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomItemUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI roomName;
    [SerializeField] TextMeshProUGUI playerCount;
    [SerializeField] Button joinButton;

    GameServerResponse serverJson;
    ServerApi serverApi;

    void Start() {
        serverApi = FindFirstObjectByType<ServerApi>();
    }

    public async Task SetServerJson(GameServerResponse server) {
        serverJson = server;
        roomName.text = server.ip + ":" + server.port;
        playerCount.text = server.players + "/" + server.capacity;
    }

    public async void JoinServer() {
        joinButton.interactable = false;

        Debug.Log("Calling API...");
        bool exists = await serverApi.GetServer(serverJson.ip, serverJson.port);
        if (!exists) {
            Debug.LogWarning("Server doesn't exist!");
            return;
        }

        Debug.Log($"Server found! Joining {serverJson.ip}:{serverJson.port}...");
        NetworkManager.singleton.networkAddress = serverJson.ip;
        NetworkManager.singleton.GetComponent<KcpTransport>().Port = (ushort)serverJson.port;
        NetworkManager.singleton.StartClient();
    }
}

[System.Serializable]
public class GameServerResponse {
    public string ip;
    public int port;
    public string state;
    public int players;
    public int capacity;

    public override string ToString() {
        return "{" + $"\"ip\":\"{ip}\",\"port\":{port},\"state\":\"{state}\",\"players\":{players},\"capacity\":{capacity}" + "}";
    }
}

// wrapper for json arrays
[System.Serializable]
public class JsonWrapper<T> {
    public T[] Items;
}

public static class JsonHelper {
    public static T[] ParseArray<T>(string json) {
        string wrappedJson = "{\"Items\":" + json + "}";
        return JsonUtility.FromJson<JsonWrapper<T>>(wrappedJson).Items;
    }
}
