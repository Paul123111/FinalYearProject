using UnityEngine;

[CreateAssetMenu(fileName = "DefaultBullet", menuName = "Projectile/DefaultBullet")]
public class ProjectileProperties : ScriptableObject {
    
    // Bullet stats
    [SerializeField] float _damage = 10f;
    [SerializeField] bool _destroyOnHit = true;
    [SerializeField] float _speed = 20f;
    
    // Bullet appearance
    [SerializeField] Sprite _sprite;
    [SerializeField] float _size = 0.2f;

    public float damage {get {return _damage;}}
    public bool destroyOnHit {get {return _destroyOnHit;}}
    public float speed {get {return _speed;}}
    public Sprite sprite {get {return _sprite;}}
    public float size {get {return _size;}}
}
