using UnityEngine;

public class HealthSystem : MonoBehaviour
{

    //Collider2D hitbox;
    float _health = 1;
    //Hitbox _lastHitBy;
    bool _alive = true;

    [SerializeField] Transform pivot;
    [SerializeField] RectTransform uiPivot;
    UnitStats stats;

    void Start()
    {
        _health = stats.maxhealth;
    }

    // negative diff = damage, positive = heal
    public void ChangeHealth(float diff) {
        _health+=diff;
        if (_health > stats.maxhealth) {
            _health = stats.maxhealth;
        } else if (_health <= 0) {
            _health=0;
            pivot.localScale = new Vector3(_health/stats.maxhealth, 1, 1);
            if (uiPivot != null) {
                uiPivot.localScale = new Vector3(_health/stats.maxhealth, 1, 1);
            }
            Die();
        }
        pivot.localScale = new Vector3(_health/stats.maxhealth, 1, 1);
        if (uiPivot != null) {
            uiPivot.localScale = new Vector3(_health/stats.maxhealth, 1, 1);
        }
    }

   // public void Damage(Hitbox hitbox) {
   //     if (!_alive) return;
   //     _lastHitBy = hitbox;
   //     _health -= hitbox.GetDamage();
   //     if (_health <= 0) {
   //         _alive = false;
   //         _health = 0;
   //     }
   // }

    void Die() {
        transform.parent.gameObject.active = false;
    }

    public void SetStats(UnitStats s) {
        stats = s;
    }
}
