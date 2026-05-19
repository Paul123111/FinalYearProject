using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class aiShoot : NetworkBehaviour
{
    public float moveSpeed = 5f;

    [SerializeField] GameObject projectile;
    [SerializeField] Transform gun;

    float attacking = 1;
    [SerializeField] float cooldown;
    float serverFireTime = 0;

    LookSpriteSheet l;

    public override void OnStartClient() {
        l = GetComponent<LookSpriteSheet>();
    }

    public override void OnStartServer() {
        l = GetComponent<LookSpriteSheet>();
    }

    void Fire() {
        int angle = GetMouseAngle();
        CmdSpawnBullet(angle);
    }

    [Server]
    void CmdSpawnBullet(int angle) {
        if (projectile == null || gun == null) return;
        if (serverFireTime > Time.time - 0.05f) return; // buffer + authoritity

        serverFireTime = Time.time + cooldown;

        GameObject bullet = Instantiate(projectile, gun.position, Quaternion.Euler(0, 0, angle));
        NetworkProjectile projScript = bullet.GetComponent<NetworkProjectile>();

        if (projScript != null) {
            projScript.Setup(true, -1, Vector3.zero);
        }
        NetworkServer.Spawn(bullet);
    }

    [ServerCallback]
    void FixedUpdate() {
        if (attacking != 0 && serverFireTime < Time.time) {
            Fire();
        }

    }

    int GetMouseAngle() {
        return l.angle;
    }
}
