using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitMovement : MonoBehaviour
{
    public UnitStats stats;
    Vector3 direction = new Vector3(0, 0);
    Vector3 targetPos = new Vector3(0, 0);
    [SerializeField] Transform body;
    [SerializeField] Transform head;

    Tilemap worldMap;
    Vector3 worldBounds;

    int step = 5;

    float tileMultiplier = 1f;

    void Start() {
        worldMap = GameObject.Find("Ground").GetComponent<Tilemap>();
        worldMap.CompressBounds();
        worldBounds = new Vector3(50, 50, 1);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += direction * stats.SpeedMultiplier() * step * Time.deltaTime * tileMultiplier;
        LookAtTarget();
        LoopAtEdge();
        GetTileEffect();
    }

    void LookAtTarget() {
        Vector3 newPos = targetPos;
        newPos -= transform.position;
        float angle = Mathf.Atan2(newPos.y, newPos.x) * Mathf.Rad2Deg;
        if (angle < 90 && angle > -90) {
            head.localScale = new Vector3(1, 1, 1);
            head.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        } else {
            head.localScale = new Vector3(-1, 1, 1);
            head.rotation = Quaternion.Euler(new Vector3(0, 0, angle-180));
        }
    }

    public void SetTarget(Vector3 target) {
        targetPos = target;
    }

    public void SetDirection(Vector3 vec) {
        direction = vec;
    }

    public void SetStats(UnitStats s) {
        stats = s;
    }

    void LoopAtEdge() {
        Vector3 pos = transform.position;
        if (pos.x < -worldBounds.x) {
            transform.position = new Vector3(worldBounds.x, pos.y, pos.z);
        } else if (pos.x > worldBounds.x) {
            transform.position = new Vector3(-worldBounds.x, pos.y, pos.z);
        } else if (pos.y < -worldBounds.y) { 
            transform.position = new Vector3(pos.x, worldBounds.y, pos.z);
        } else if (pos.y > worldBounds.y) {
            transform.position = new Vector3(pos.x, -worldBounds.y, pos.z);
        }
        //Debug.Log(-10%100);
    }

    void GetTileEffect() {
        string? tileType = worldMap.GetTile(worldMap.WorldToCell(transform.position))?.name;
        switch(tileType) {
            case "PrototypeTiles_0": tileMultiplier = 0.5f; break;
            default: tileMultiplier = 1f; break;
        }
    }
}

