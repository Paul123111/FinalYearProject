using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    public UnitStats stats;
    Vector3 direction = new Vector3(0, 0);
    Vector3 targetPos = new Vector3(0, 0);
    [SerializeField] Transform body;
    [SerializeField] Transform head;

    int step = 5;

    // Update is called once per frame
    void Update()
    {
        transform.position += direction * stats.SpeedMultiplier() * step * Time.deltaTime;
        LookAtTarget();
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
}

