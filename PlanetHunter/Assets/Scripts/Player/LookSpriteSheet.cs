using UnityEngine;
using UnityEngine.InputSystem;

public class LookSpriteSheet : MonoBehaviour
{

    int angle;
    int direction; // 0-3 based on diagonal quadrants: -45 to 45 is first, etc.
    Animator[] anims;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anims = GetComponentsInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        angle = GetMouseAngle();
        direction = (angle-45)/90;
        anims[0].SetInteger("Direction", direction);
        anims[1].SetInteger("Direction", direction);
        Debug.Log("Angle: " + angle + ", Direction: " + direction);
    }

    int GetMouseAngle() {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (Vector2)mouseWorldPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) { angle += 360f; }
        return ((int)angle);
    }
}
