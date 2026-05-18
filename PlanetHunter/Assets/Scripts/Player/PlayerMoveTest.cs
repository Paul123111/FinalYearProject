using Mirror;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem; // Required for the new system

public class PlayerMoveTest : NetworkBehaviour {
    public float moveSpeed = 5f;

    private Rigidbody2D body;
    private Vector2 moveInput;

    [SerializeField] GameObject projectile;
    [SerializeField] Transform gun;

    [SerializeField] float cooldown;

    void Awake() {
        body = GetComponent<Rigidbody2D>();
    }

    public async override void OnStartAuthority() {
        base.OnStartAuthority();
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
    }

    void OnMove(InputValue value) {
        if (!isLocalPlayer) return;
        moveInput = value.Get<Vector2>();
    }

    void OnAttack() {
        if (!isLocalPlayer) return;
        int angle = GetMouseAngle();
        Vector2 v = body.linearVelocity;
        CmdSpawnBullet(angle, v);
    }

    [Command]
    void CmdSpawnBullet(int angle, Vector2 playerVelocity) {
        if (projectile == null || gun == null) return;

        GameObject bullet = Instantiate(projectile, gun.position, Quaternion.Euler(0, 0, angle));
        NetworkProjectile projScript = bullet.GetComponent<NetworkProjectile>();

        if (projScript != null) {
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null) {
                Vector2 fireDirection = bullet.transform.right;
                bulletRb.linearVelocity = (fireDirection * projScript.speed) + playerVelocity;
            }
        }
        NetworkServer.Spawn(bullet);
    }

    void FixedUpdate() {
        if (!isLocalPlayer) return;
        body.linearVelocity = moveInput * moveSpeed;
    }

    int GetMouseAngle() {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (Vector2)mouseWorldPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) { angle += 360f; }
        return ((int)angle);
    }
}