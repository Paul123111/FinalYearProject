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

    [SerializeField] AudioSource audio;

    public void Awake() {
        eq = GetComponent<EquipmentSlots>();
    }

    public override void OnStartClient() {
        base.OnStartClient();
        InitialiseGun();
    }

    public override void OnStartServer() {
        base.OnStartServer();
        InitialiseGun();
    }

    void OnEnable() {
        if (eq != null) {
            eq.OnEquipmentChanged += ChangeGun;
        }
    }

    void InitialiseGun() {
        if (eq == null) {
            eq = GetComponent<EquipmentSlots>();
        }

        if (eq != null) {
            eq.RefreshAll();
            ChangeGun();
        } else {
            Debug.LogError($"Missing EquipmentSlots on {gameObject.name}!");
        }
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
            if (isServer && serverFireTime < Time.time) {
                serverFireTime = Time.time + (cooldown * 2f); // longer cooldown for enemies
                ServerSpawnBullet(angle, playerNum);
            } else if (isPlayer) {
                if (audio != null) {
                    audio.Play();
                }
                CmdSpawnBullet(angle, playerNum);
            }
        }
    }

    [Command]
    void CmdSpawnBullet(int angle, int colourNum) {
        if (projectile == null || gunMuzzle == null) return;
        if (serverFireTime > Time.time - 0.01f) return; // buffer + authority

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

        GameObject bullet = Instantiate(projectile, gunMuzzle.position, Quaternion.Euler(0, 0, angle));
        NetworkProjectile projScript = bullet.GetComponent<NetworkProjectile>();

        if (projScript != null) {
            projScript.Setup(!isPlayer, colourNum, projectileProps);
        }
        NetworkServer.Spawn(bullet);
    }
}
