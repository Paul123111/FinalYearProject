using Mirror;
using UnityEngine;

public class HealthSystemN : NetworkBehaviour
{
    //Collider2D hitbox;
    [SyncVar(hook = nameof(OnHealthChanged))] int _health = 1;
    [SerializeField] int maxHealth = 100;
    //Hitbox _lastHitBy;
    bool _alive = true;

    [SerializeField] Transform pivot;
    [SerializeField] RectTransform uiPivot;
    //UnitStats stats;
    float maxRegenCooldown = 0.5f;
    float regenCooldown = 0f;

    public override void OnStartServer() {
        base.OnStartServer();
        _health = maxHealth;
    }

    void Update() {
        //Regen();
    }

    void OnHealthChanged(int oldHealth, int newHealth) {
        //if (newHealth > maxHealth) {
        //    newHealth = maxHealth;
        //} else 
        if (isServer && newHealth <= 0) {
            pivot.localScale = new Vector3((float) newHealth / (float) maxHealth, 1, 1);
            if (uiPivot != null) {
                uiPivot.localScale = new Vector3(newHealth / (float) maxHealth, 1, 1);
            }
            Die();
            return;
        }
        pivot.localScale = new Vector3((float)newHealth / (float) maxHealth, 1, 1);
        if (uiPivot != null) {
            uiPivot.localScale = new Vector3((float)newHealth / (float) maxHealth, 1, 1);
        }
    }

    [Server]
    public void Damage(int diff) {
        if (isServer) {
            _health -= diff;
        }
    }

    [Server]
    void Die() {
        NetworkServer.Destroy(gameObject);
    }

    // negative diff = damage, positive = heal
    //public void ChangeHealth(float diff) {
    //    _health += diff * (1f - stats.DamageResistance());
    //    if (_health > maxHealth) {
    //        _health = maxHealth;
    //    } else if (_health <= 0) {
    //        _health = 0;
    //        pivot.localScale = new Vector3(_health / maxHealth, 1, 1);
    //        if (uiPivot != null) {
    //            uiPivot.localScale = new Vector3(_health / maxHealth, 1, 1);
    //        }
    //        Die();
    //    }
    //    pivot.localScale = new Vector3(_health / maxHealth, 1, 1);
    //    if (uiPivot != null) {
    //        uiPivot.localScale = new Vector3(_health / maxHealth, 1, 1);
    //    }
    //}

    //void Regen() {
    //    regenCooldown -= Time.deltaTime;
    //    if (regenCooldown <= 0) {
    //        regenCooldown = maxRegenCooldown;
    //        ChangeHealth(stats.Regen());
    //    }
    //}

    // public void Damage(Hitbox hitbox) {
    //     if (!_alive) return;
    //     _lastHitBy = hitbox;
    //     _health -= hitbox.GetDamage();
    //     if (_health <= 0) {
    //         _alive = false;
    //         _health = 0;
    //     }
    // }



    //public void SetStats(UnitStats s) {
    //    stats = s;
    //}
}
