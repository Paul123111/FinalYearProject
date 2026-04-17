using UnityEngine;
using UnityEngine.Pool;
using static LoopHelper;

public class ProjectileScript : MonoBehaviour, IPoolObject
{
    
    float speed = 5f;
    float maxLifetime = 3f;
    float lifetime;
    ObjectPool<ProjectileScript> _pool;
    DamageHitbox hitbox;
    SpriteRenderer _renderer;

    void Awake()
    {
        hitbox = GetComponent<DamageHitbox>();
        _renderer = GetComponent<SpriteRenderer>();
        lifetime = maxLifetime;
    }

    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
        transform.position = LoopPos(transform.position, 100, 100);
        lifetime -= Time.deltaTime;
        if (lifetime <= 0) {
            ReturnToPool();
        }
    }

    public void Setup(ObjectPool<ProjectileScript> pool, Transform t, bool isEnemy, ProjectileProperties props) {
        hitbox.SetEnemy(isEnemy);
        _pool = pool;
        lifetime = maxLifetime;
        speed = props.speed;
        hitbox.damage = props.damage;
        hitbox.destroyOnHit = props.destroyOnHit;
        _renderer.sprite = props.sprite;
        transform.localScale = new Vector3(props.size, props.size, 1);
        transform.rotation = t.rotation;
        Debug.Log(hitbox.damage);
    }

    public void ActivateAt(Vector3 worldPos) { 
        transform.position = worldPos;
        gameObject.SetActive(true);
    }

    public void ReturnToPool() {
        if (_pool != null) {
            _pool.Release(this);
        } else {
            // Safety fallback
            gameObject.SetActive(false);
        }
    }
}
