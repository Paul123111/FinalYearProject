using UnityEngine;
using Mirror;
public struct PlayerAuthMessage : NetworkMessage
{
    public string id;
    public string token;
}
