using Mirror;
using ProcGen;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem; // Required for the new system

public class PlayerMoveTest : NetworkBehaviour {
    public float moveSpeed = 5f;

    private Rigidbody2D body;
    private Vector2 moveInput;

    GunCombat gunCombat;
    PlayerColour playerColour;
    ProcGenNetworking procGen;

    void Awake() {
        body = GetComponent<Rigidbody2D>();
        playerColour = GetComponentInChildren<PlayerColour>();
        gunCombat = GetComponent<GunCombat>();
    }

    public async override void OnStartAuthority() {
        base.OnStartAuthority();
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
        procGen = GameObject.Find("ProcGenWalls")?.GetComponent<ProcGenNetworking>();
        if (procGen != null) {
            RandomSpawn();
        }
        gunCombat.playerNum = (int)playerColour.playerNum;
    }

    void OnMove(InputValue value) {
        if (!isLocalPlayer) return;
        moveInput = value.Get<Vector2>();
    }

    void OnAttack(InputValue v) {
        if (!isLocalPlayer) return;
        gunCombat.attacking = v.Get<float>();
    }

    void FixedUpdate() {
        if (!isLocalPlayer) return;
        body.linearVelocity = moveInput * moveSpeed;
        gunCombat.angle = GetMouseAngle();
    }

    int GetMouseAngle() {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (Vector2)mouseWorldPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) { angle += 360f; }
        return ((int)angle);
    }

    void RandomSpawn() {
        if (procGen != null) {
            transform.position = procGen.SpawnPoint();
        }
    }
}