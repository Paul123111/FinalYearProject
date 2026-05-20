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

    [SerializeField] GameObject projectile;
    [SerializeField] Transform gun;

    float attacking = 0;
    [SerializeField] float cooldown;
    float clientFireTime = 0;
    float serverFireTime = 0;

    PlayerColour playerColour;
    ProcGenNetworking procGen;

    void Awake() {
        body = GetComponent<Rigidbody2D>();
        playerColour = GetComponentInChildren<PlayerColour>();
    }

    public async override void OnStartAuthority() {
        base.OnStartAuthority();
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
        procGen = GameObject.Find("ProcGenWalls")?.GetComponent<ProcGenNetworking>();
        if (procGen != null) {
            RandomSpawn();
        }
    }

    void OnMove(InputValue value) {
        if (!isLocalPlayer) return;
        moveInput = value.Get<Vector2>();
    }

    void OnAttack(InputValue v) {
        if (!isLocalPlayer) return;
        attacking = v.Get<float>();
    }

    void Fire() {
        int angle = GetMouseAngle();
        CmdSpawnBullet(angle);
    }

    [Command]
    void CmdSpawnBullet(int angle) {
        if (projectile == null || gun == null) return;
        if (serverFireTime > Time.time - 0.05f) return; // buffer + authoritity

        serverFireTime = Time.time + cooldown;

        GameObject bullet = Instantiate(projectile, gun.position, Quaternion.Euler(0, 0, angle));
        NetworkProjectile projScript = bullet.GetComponent<NetworkProjectile>();

        if (projScript != null) {
            projScript.Setup(false, playerColour.playerNum, body.linearVelocity);
        }
        NetworkServer.Spawn(bullet);
    }

    void FixedUpdate() {
        if (!isLocalPlayer) return;
        body.linearVelocity = moveInput * moveSpeed;
        if (attacking != 0 && clientFireTime < Time.time) {
            clientFireTime = Time.time + cooldown;
            Fire();
        }
        
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