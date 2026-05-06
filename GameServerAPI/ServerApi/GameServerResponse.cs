using System.Text.Json;
using System.Text.Json.Serialization;

public record GameServerResponse(
    [property: JsonPropertyName("ip")] string Ip,
    [property: JsonPropertyName("port")] int Port,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("players")] int Players,
    [property: JsonPropertyName("capacity")] int Capacity
);

public static class GameServerResponseUtils {
    public static GameServerResponse ParseJson(JsonElement item) {
        var status = item.GetProperty("status");

        var ip = status.GetProperty("ip").GetString();
        var port = status.GetProperty("port").GetInt32();
        var state = status.GetProperty("state").GetString();
        
        var players = status.GetProperty("players");
        var count = players.GetProperty("count").GetInt32();
        var capacity = players.GetProperty("capacity").GetInt32();

        return new GameServerResponse(ip, port, state, count, capacity);
    }
}