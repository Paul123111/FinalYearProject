using Mirror;
using UnityEngine;

public class HealthSystemN : NetworkBehaviour
{
    //Collider2D hitbox;
    [SyncVar(hook = nameof(OnHealthChanged))] public int _health = 1;
    public int maxHealth = 100;

    [SerializeField] Transform pivot;
    [SerializeField] RectTransform uiPivot;

    [SerializeField] [SyncVar] bool isEnemy = false;
    WinCondition win;

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
        if (isEnemy) {
            Debug.Log("enemy died");
            if (win == null) {
                win = WinCondition.Instance;
                if (win == null) {
                    win = FindAnyObjectByType<WinCondition>();
                }
            }

            // Safe execution verification
            if (win != null) {
                win.IncCount();
            } else {
                Debug.LogError($"Enemy died but could not find a active WinCondition manager in the scene!");
            }
        }
        NetworkServer.Destroy(gameObject);
    }
}
