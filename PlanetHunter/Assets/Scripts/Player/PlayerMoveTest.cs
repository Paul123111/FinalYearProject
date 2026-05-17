using Mirror;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem; // Required for the new system

public class PlayerMoveTest : NetworkBehaviour {
    public float moveSpeed = 5f;

    private Rigidbody2D body;
    private Vector2 moveInput;

    NetworkTransformReliable nt;

    void Awake() {
        body = GetComponent<Rigidbody2D>();
        nt = GetComponent<NetworkTransformReliable>();
    }
    public async override void OnStartAuthority() {
        base.OnStartAuthority();
        await Task.Delay(500);
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
    }

    void OnMove(InputValue value) {
        if (!isLocalPlayer) return;
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate() {
        if (!isLocalPlayer) return;
        body.linearVelocity = moveInput * moveSpeed;
    }
}