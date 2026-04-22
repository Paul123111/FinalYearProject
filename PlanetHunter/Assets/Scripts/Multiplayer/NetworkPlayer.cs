using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class NetworkPlayer : NetworkBehaviour
{
    Rigidbody2D body;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
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

    [Command] void SyncPos() {
        //t.transform.position = transform.position;
    }

    void OnMove(InputValue value) {
        if (!isLocalPlayer) return;
        Vector2 move = value.Get<Vector2>();
        body.linearVelocity = new Vector3(move.x, move.y, 0);
    }
}
