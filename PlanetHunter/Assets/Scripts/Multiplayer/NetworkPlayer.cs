using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using Agones;

public class NetworkPlayer : NetworkBehaviour
{
    Rigidbody2D body;
    NetworkTransformReliable networkTransform;
    AgonesAlphaSdk agones;
    AgonesStartup startup;

    float rotateDir;
    float moveforward;
    [SerializeField] float rotateSpeed = 1f;
    [SerializeField] float moveSpeed = 1f;

    [SerializeField] int worldWidth = 22;
    [SerializeField] int worldHeight = 12;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        body = GetComponent<Rigidbody2D>();
        networkTransform = GetComponent<NetworkTransformReliable>();
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
        body.rotation += rotateSpeed * rotateDir * Time.deltaTime;
        body.linearVelocity = transform.up * moveforward * moveSpeed;
        LoopHelper.LoopPosNetwork(transform, worldWidth, worldHeight, networkTransform);
    }

    void OnMove(InputValue value) {
        if (!isLocalPlayer) return;
        Vector2 move = value.Get<Vector2>();
        rotateDir = -move.x;
        moveforward = move.y > 0 ? move.y : 0;
    }
}
