using TMPro;
using UnityEngine;

public class RoomItemUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI roomName;
    [SerializeField] TextMeshProUGUI playerCount;

    GameServerResponse serverJson;

    public void SetServerJson(GameServerResponse server) {
        serverJson = server;
        roomName.text = server.ip + ":" + server.port;
        playerCount.text = server.players + "/" + server.capacity;
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
