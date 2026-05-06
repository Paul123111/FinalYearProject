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
    public static GameServerResponse ParseGameServerJson(JsonElement item) {
        var status = item.GetProperty("status");

        var ip = status.GetProperty("address").GetString();
        var ports = status.GetProperty("port");
        var port = ports[0].GetProperty("port").GetInt32();
        var state = status.GetProperty("state").GetString();
        
        var players = status.GetProperty("players");
        var count = players.GetProperty("count").GetInt32();
        var capacity = players.GetProperty("capacity").GetInt32();

        return new GameServerResponse(ip, port, state, count, capacity);
    }

    public static GameServerResponse ParseAllocationJson(JsonElement item) {
        var status = item.GetProperty("status");

        var ip = status.GetProperty("address").GetString();
        var ports = status.GetProperty("port");
        var port = ports[0].GetProperty("port").GetInt32();
        var state = status.GetProperty("state").GetString();

        return new GameServerResponse(ip, port, state, -1, -1);
    }
}