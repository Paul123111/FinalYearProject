using Mirror;
using UnityEngine;

public class HealthSystemN : NetworkBehaviour
{
    //Collider2D hitbox;
    [SyncVar(hook = nameof(OnHealthChanged))] public int _health = 1;
    public int maxHealth = 100;

    [SerializeField] Transform pivot;
    [SerializeField] RectTransform uiPivot;

    public override void OnStartServer() {
        base.OnStartServer();
        _health = maxHealth;
    }

    void OnHealthChanged(int oldHealth, int newHealth) {
        CheckHealth();
    }

    [Server]
    public void Damage(int diff) {
        if (_health <= 0) { return; }
        _health -= diff;
        CheckHealth();
    }

    void CheckHealth() {
        if (_health <= 0) {
            pivot.localScale = new Vector3((float)_health / (float)maxHealth, 1, 1);
            if (uiPivot != null) {
                uiPivot.localScale = new Vector3(_health / (float)maxHealth, 1, 1);
            }
            if (isServer) {
                Die();
            }
            return;
        }
        pivot.localScale = new Vector3((float)_health / (float)maxHealth, 1, 1);
        if (uiPivot != null) {
            uiPivot.localScale = new Vector3((float)_health / (float)maxHealth, 1, 1);
        }
    }

    [Server]
    void Die() {
        NetworkServer.Destroy(gameObject);
    }
}
