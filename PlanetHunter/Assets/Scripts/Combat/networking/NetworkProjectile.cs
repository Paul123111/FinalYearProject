using Mirror;
using UnityEngine;
using UnityEngine.Pool;
using static LoopHelper;

public class NetworkProjectile : NetworkBehaviour {
    public float speed = 15f;
    [SerializeField] float maxLifetime = 3f;
    float lifetime;
    ObjectPool<ProjectileScript> _pool;
    DamageHitbox hitbox;
    SpriteRenderer _renderer;
    [SerializeField] LayerMask targetLayerMask;
    [SerializeField] bool isEnemy = false;

    void Start() {
        hitbox = GetComponent<DamageHitbox>();
        _renderer = GetComponent<SpriteRenderer>();
        lifetime = maxLifetime;
    }

    void FixedUpdate() {
        transform.position += transform.right * speed * Time.fixedDeltaTime;
        lifetime -= Time.fixedDeltaTime;
        if (lifetime <= 0) {
            Destroy(gameObject);
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision) {
        if (((1 << collision.gameObject.layer) & targetLayerMask) != 0) {
            NetworkServer.Destroy(gameObject);
        }
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
