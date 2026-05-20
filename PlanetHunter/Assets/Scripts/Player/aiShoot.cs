using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class aiShoot : NetworkBehaviour
{
    GunCombat gunCombat;
    [SerializeField] float moveSpeed = 7f;
    [SerializeField] float aggroRange = 1f;
    [SerializeField] LayerMask playerLayer;
    List<Collider2D> players = new List<Collider2D>();
    ContactFilter2D contactFilter;
    Rigidbody2D rb;
    [SyncVar] bool chase = false;
    LookSpriteSheet lookSpriteSheet;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        lookSpriteSheet = GetComponent<LookSpriteSheet>();
        gunCombat = GetComponent<GunCombat>();
        contactFilter.useLayerMask = true;
        contactFilter.SetLayerMask(playerLayer);
    }

    public override void OnStartServer() {
        base.OnStartServer();
        StartCoroutine(Detect());
    }

    [ServerCallback]
    private void GetNearestPlayerInRange() {
        int playersNearby = Physics2D.OverlapCircle(transform.position, aggroRange, contactFilter, players);
        if (playersNearby <= 0) { chase = false; return; }
        Debug.Log(playersNearby + " detected");
        chase = true;
        players.Sort(0, playersNearby, Comparer<Collider2D>.Create((a, b) => {
            float distA = (a.transform.position - transform.position).sqrMagnitude;
            float distB = (b.transform.position - transform.position).sqrMagnitude;
            return distA.CompareTo(distB);
        }));
    }

    public void Update() {
        if (isServer && chase && players.Count > 0) {
            Vector3 direction = (players[0].transform.position-transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
            gunCombat.attacking = 1;
            gunCombat.angle = GetAngle(direction);
            lookSpriteSheet.angle = GetAngle(direction);
        } else if (isServer) {
            rb.linearVelocity = Vector3.zero;
            gunCombat.attacking = 0;
        }
    }

    int GetAngle(Vector2 direction) {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) { angle += 360f; }
        return ((int)angle);
    }

    System.Collections.IEnumerator Detect() {
        yield return new WaitForSeconds(1f);
        for (;;) {
            GetNearestPlayerInRange();
            yield return new WaitForSeconds(0.2f);
        }
    }
}
