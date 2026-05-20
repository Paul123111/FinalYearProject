using Items;
using Mirror;
using UnityEngine;

public class NetworkProjectile : NetworkBehaviour {
    float lifetime = 0;
    [SerializeField] LayerMask targetLayerMask;
    [SerializeField] LayerMask playerLayerMask;
    [SerializeField] LayerMask enemyLayerMask;
    [SyncVar] bool isEnemy = false;
    Rigidbody2D rb;
    PlayerColour playerColour;
    [SerializeField] ProjectilePropertiesN props;
    public float currScale; // bullet grows in size
    SpriteRenderer spriteRenderer;
    [SyncVar(hook = nameof(ChangeProjectile))] private int syncProjId = 0;
    [SerializeField] EquipmentDatabase equipmentDatabase;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerColour = GetComponent<PlayerColour>();
    }

    void Start() {
        if (props == null) return;
        lifetime = props.maxLifetime;
        spriteRenderer.sprite = props.sprite;
    }

    void FixedUpdate() {
        if (props == null) return;
        lifetime -= Time.fixedDeltaTime;
        if (currScale < props.maxScale) {
            currScale += props.scaleRateOfChange * Time.fixedDeltaTime;
            transform.localScale = new Vector3(currScale, currScale, 1);
        }
        if (lifetime <= 0) {
            DestroyBullet();
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision) {
        if (((1 << collision.gameObject.layer) & targetLayerMask) != 0) {
            DestroyBullet();
        } else if (!isEnemy && ((1 << collision.gameObject.layer) & enemyLayerMask) != 0) {
            collision.GetComponent<HealthSystemN>()?.Damage(props.damage);
            if (props.destroyOnHit) DestroyBullet();
        } else if (isEnemy && ((1 << collision.gameObject.layer) & playerLayerMask) != 0) {
            collision.GetComponent<HealthSystemN>()?.Damage(props.damage);
            if (props.destroyOnHit) DestroyBullet();
        }
    }

    private void DestroyBullet() {
        if (isServer) {
            NetworkServer.Destroy(gameObject);
        }
    }

    public void Setup(bool enemyProjectile, long playerNum, ProjectilePropertiesN p) {
        isEnemy = enemyProjectile;
        playerColour.playerNum = playerNum;
        syncProjId = p.id;
        props = p;
        rb.linearVelocity = (transform.right * p.speed);
    }

    public void ChangeProjectile(int old, int id) {
        props = equipmentDatabase.GetProjectileByIndex(id);
    }
}
