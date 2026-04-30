using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using Agones;

public class NetworkPlayer : NetworkBehaviour
{
    Rigidbody2D body;
    NetworkTransformReliable networkTransform;
    Animator anim;

    float rotateDir;
    float moveforward;
    [SerializeField] float rotateSpeed = 1f;
    [SerializeField] float moveSpeed = 1f;

    [SerializeField] int worldWidth = 22;
    [SerializeField] int worldHeight = 12;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        networkTransform = GetComponent<NetworkTransformReliable>();
        anim = GetComponent<Animator>();
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
        if (move.y > 0) {
            anim.SetBool("RocketOn", true);
        } else {
            anim.SetBool("RocketOn", false);
        }
        moveforward = move.y > 0 ? move.y : 0;
    }

    [ServerCallback]
    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Player")) {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 pushDir = collision.contacts[0].point - transform.position;
            rb.AddForce(-pushDir.normalized, ForceMode.Impulse);
        }
    }

}
