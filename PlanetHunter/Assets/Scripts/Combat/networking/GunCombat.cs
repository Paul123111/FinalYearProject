using Items;
using Mirror;
using ProcGen;
using UnityEngine;

public class GunCombat : NetworkBehaviour
{
    [SerializeField] GameObject projectile;
    [SerializeField] Transform gunMuzzle;
    [SerializeField] ProjectilePropertiesN projectileProps;
    float cooldown;
    EquipmentSlots eq;
    [SerializeField] bool isPlayer = true;

    float clientFireTime = 0;
    float serverFireTime = 0;

    public float attacking = 0;
    public int angle = 0;
    public int playerNum = 0;

    public override void OnStartClient() {
        if (eq != null) {
            eq.RefreshAll();
            ChangeGun();
        }
    }

    void OnEnable() {
        eq = GetComponent<EquipmentSlots>();
        eq.OnEquipmentChanged += ChangeGun;
    }

    void OnDisable() {
        if (eq != null) {
            eq.OnEquipmentChanged -= ChangeGun;
        }
    }

    void ChangeGun() {
        if (eq == null) return;
        if (eq.gun != null) {
            projectileProps = eq.gun.props;
            cooldown = eq.gun.cooldown;
        }
    }

    public void Update() {
        if (attacking != 0 && clientFireTime < Time.time) {
            clientFireTime = Time.time + cooldown;
            if (isServer) {
                ServerSpawnBullet(angle, playerNum);
            } else {
                CmdSpawnBullet(angle, playerNum);
            }
        }
    }

    [Command]
    void CmdSpawnBullet(int angle, int colourNum) {
        if (projectile == null || gunMuzzle == null) return;
        if (serverFireTime > Time.time - 0.05f) return; // buffer + authority

        serverFireTime = Time.time + cooldown;

        GameObject bullet = Instantiate(projectile, gunMuzzle.position, Quaternion.Euler(0, 0, angle));
        NetworkProjectile projScript = bullet.GetComponent<NetworkProjectile>();

        if (projScript != null) {
            projScript.Setup(!isPlayer, colourNum, projectileProps);
        }
        NetworkServer.Spawn(bullet);
    }

    [Server]
    void ServerSpawnBullet(int angle, int colourNum) {
        if (projectile == null || gunMuzzle == null) return;
        if (serverFireTime > Time.time - 0.05f) return; // buffer + authority

        serverFireTime = Time.time + cooldown;

        GameObject bullet = Instantiate(projectile, gunMuzzle.position, Quaternion.Euler(0, 0, angle));
        NetworkProjectile projScript = bullet.GetComponent<NetworkProjectile>();

        if (projScript != null) {
            projScript.Setup(false, colourNum, projectileProps);
        }
        NetworkServer.Spawn(bullet);
    }
}
