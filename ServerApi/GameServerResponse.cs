using System.Text.Json;
using System.Text.Json.Serialization;

public record GameServerResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("ip")] string Ip,
    [property: JsonPropertyName("port")] int Port,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("players")] int Players,
    [property: JsonPropertyName("capacity")] int Capacity
);

public static class GameServerResponseUtils {
    public static GameServerResponse ParseGameServerJson(JsonElement item) {
        int count = 0;
        int capacity = 0;
        string displayName = "Unknown";
        string ip = "0.0.0.0";
        int port = 0;
        string state = "Unknown";

        if (item.TryGetProperty("status", out JsonElement status)) {
            if (status.TryGetProperty("address", out var addr)) {
                ip = addr.GetString() ?? ip;
            }
            if (status.TryGetProperty("ports", out var ports) && ports.ValueKind == JsonValueKind.Array && ports.GetArrayLength() > 0 && ports.EnumerateArray().FirstOrDefault().TryGetProperty("port", out var p)) {
                port = p.GetInt32();
            }
            if (status.TryGetProperty("state", out var s)) {
                state = s.GetString() ?? state;
            }
            if (item.TryGetProperty("metadata", out var metadata) && metadata.TryGetProperty("labels", out var labels) && labels.TryGetProperty("game.display-name", out var n)) {
                displayName = n.GetString() ?? displayName;
            }
            if (status.TryGetProperty("players", out var players)) {
                if (players.TryGetProperty("count", out var c)) {
                    count = c.GetInt32();
                }
                if (players.TryGetProperty("capacity", out var d)) {
                    capacity = d.GetInt32();
                }
            }
        }

        return new GameServerResponse(displayName, ip, port, state, count, capacity);
    }

    public static GameServerResponse ParseAllocationJson(JsonElement item) {
        string displayName = "Unknown";
        string ip = "0.0.0.0";
        int port = 0;
        string state = "Unknown";

        if (item.TryGetProperty("status", out JsonElement status)) {
            if (status.TryGetProperty("address", out var addr)) {
                ip = addr.GetString() ?? ip;
            }
            if (status.TryGetProperty("ports", out var ports) && ports.ValueKind == JsonValueKind.Array && ports.GetArrayLength() > 0 && ports.EnumerateArray().FirstOrDefault().TryGetProperty("port", out var p)) {
                port = p.GetInt32();
            }
            if (status.TryGetProperty("state", out var s)) {
                state = s.GetString() ?? state;
            }
            if (item.TryGetProperty("metadata", out var metadata) && metadata.TryGetProperty("labels", out var labels) && labels.TryGetProperty("game.display-name", out var n)) {
                displayName = n.GetString() ?? displayName;
            }
        }

        return new GameServerResponse(displayName, ip, port, state, -1, -1);
    }

    public static string GenerateRoomId() {
        // Creates a unique string from byte arrays (e.g., "a1B2")
        string base64 = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        string clean = base64.Replace("/", "").Replace("+", "").Replace("=", "");
        return clean.Substring(0, 4);
    }
}