using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using Agones;

public class NetworkPlayer : NetworkBehaviour
{
    Rigidbody2D body;
    AgonesAlphaSdk agones;
    AgonesStartup startup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        body = GetComponent<Rigidbody2D>();
        agones = FindFirstObjectByType<AgonesAlphaSdk>();
        startup = FindFirstObjectByType<AgonesStartup>();
    }

    public override void OnStartAuthority() {
        base.OnStartAuthority();
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
    }

    // Update is called once per frame
    void Update() {
        if (!isLocalPlayer) return;
        //SyncPos();
    }

    void OnMove(InputValue value) {
        if (!isLocalPlayer) return;
        Vector2 move = value.Get<Vector2>();
        body.linearVelocity = new Vector3(move.x, move.y, 0);
    }

    async void OnDestroy() {
        await agones.PlayerDisconnect(startup.playerId);
    }
}
