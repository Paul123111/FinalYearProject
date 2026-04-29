using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using Agones;

public class NetworkPlayer : NetworkBehaviour
{
    Rigidbody2D body;
    AgonesAlphaSdk agones;
    AgonesStartup startup;

    float rotateDir;
    float moveforward;
    [SerializeField] float rotateSpeed = 1f;
    [SerializeField] float moveSpeed = 1f;

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
        //if (!isLocalPlayer) return;
        //SyncPos();
        body.rotation += rotateSpeed * rotateDir;
        body.linearVelocity = transform.up * moveforward * moveSpeed;
    }

    void OnMove(InputValue value) {
        if (!isLocalPlayer) return;
        Vector2 move = value.Get<Vector2>();
        rotateDir = -move.x;
        moveforward = move.y > 0 ? move.y : 0;
    }
}
