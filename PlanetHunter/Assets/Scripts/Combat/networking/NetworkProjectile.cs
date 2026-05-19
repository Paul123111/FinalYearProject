using Mirror;
using UnityEngine;

public class NetworkProjectile : NetworkBehaviour {
    public float speed = 10f;
    [SerializeField] float maxLifetime = 3f;
    float lifetime;
    DamageHitbox hitbox;
    SpriteRenderer _renderer;
    [SerializeField] LayerMask targetLayerMask;
    [SerializeField] LayerMask playerLayerMask;
    [SerializeField] LayerMask enemyLayerMask;
    [SyncVar] bool isEnemy = false;
    Rigidbody2D rb;
    PlayerColour playerColour;

    [Header("Bullet Size")]
    [SerializeField] float currScale = 0.1f; // bullet grows in size
    [SerializeField] float maxScale = 2;
    [SerializeField] float scaleRateOfChange = 2f;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start() {
        hitbox = GetComponent<DamageHitbox>();
        _renderer = GetComponent<SpriteRenderer>();
        lifetime = maxLifetime;
        maxScale = transform.localScale.x;
    }

    void FixedUpdate() {
        transform.position += transform.right * speed * Time.fixedDeltaTime;
        lifetime -= Time.fixedDeltaTime;
        if (currScale < maxScale) {
            currScale += scaleRateOfChange * Time.fixedDeltaTime;
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
            collision.GetComponent<HealthSystemN>()?.Damage(25);
            DestroyBullet();
        }
    }

    private void DestroyBullet() {
        if (isServer) {
            NetworkServer.Destroy(gameObject);
        }
    }

    public void Setup(bool enemyProjectile, long playerNum, Vector2 parentVelocity) {
        isEnemy = enemyProjectile;
        playerColour = GetComponent<PlayerColour>();
        playerColour.playerNum = playerNum;
        rb.linearVelocity = transform.right * speed;
        rb.linearVelocity += parentVelocity;
    }

    //public void Setup(ObjectPool<ProjectileScript> pool, Transform t, bool isEnemy, ProjectileProperties props) {
    //    hitbox.SetEnemy(isEnemy);
    //    _pool = pool;
    //    lifetime = maxLifetime;
    //    speed = props.speed;
    //    hitbox.damage = props.damage;
    //    hitbox.destroyOnHit = props.destroyOnHit;
    //    _renderer.sprite = props.sprite;
    //    transform.localScale = new Vector3(props.size, props.size, 1);
    //    transform.rotation = t.rotation;
    //    Debug.Log(hitbox.damage);
    //}

    //public void ActivateAt(Vector3 worldPos) {
    //    transform.position = worldPos;
    //    gameObject.SetActive(true);
    //}

    //public void ReturnToPool() {
    //    if (_pool != null) {
    //        _pool.Release(this);
    //    } else {
    //        // Safety fallback
    //        gameObject.SetActive(false);
    //    }
    //}
}
