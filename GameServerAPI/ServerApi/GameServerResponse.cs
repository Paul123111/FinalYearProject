using System.Text.Json.Serialization;

public record GameServerResponse(
    [property: JsonPropertyName("ip")] string Ip,
    [property: JsonPropertyName("port")] int Port,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("players")] int Players,
    [property: JsonPropertyName("capacity")] int Capacity
);