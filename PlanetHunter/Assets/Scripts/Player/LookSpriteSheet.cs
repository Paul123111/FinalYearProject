using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class LookSpriteSheet : MonoBehaviour
{

    int angle;
    int direction; // 0-3 based on diagonal quadrants: -45 to 45 is first, etc.
    Animator[] anims;
    [SerializeField] Transform head;
    [SerializeField] Transform body;
    [SerializeField] Transform gun;
    [SerializeField] SpriteRenderer gunRenderer;
    [SerializeField] Sprite gunHorizontal;
    [SerializeField] Sprite gunVertical;
    float myScale = 1f;
    float bodyScale = 1.5f;
    float gunScale = 1f;

    int headR = 120;
    int bodyR = 60;

    [SerializeField] bool dynamicRotation = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anims = GetComponentsInChildren<Animator>();
    }

    void Update() {
        angle = GetMouseAngle();
        //direction = ((angle + 45) / 90) % 4;
        if (angle >= 270 - (bodyR / 2) && angle < 270 + (bodyR / 2)) {
            direction = 3;
        } else if (angle >= 90 - (bodyR / 2) && angle < 90 + (bodyR / 2)) {
            direction = 1;
        } else if (angle >= 270 + (bodyR / 2) || angle < 90 - (bodyR / 2)) {
            direction = 0;
        } else {
            direction = 2;
        }
        anims[0].SetInteger("Direction", direction);
        anims[1].SetInteger("Direction", direction);

        if (!dynamicRotation) return;

        float r = -1;
        float calc = 1;
        if (direction % 2 == 0) {
            r = headR;
        } else {
            r = bodyR;
        }
        if (angle >= 270 + (bodyR / 2) || angle < 90 - (bodyR / 2)) {
            calc = ((angle + (r / 2)) % r) - (r / 2);
        } else {
            calc = ((angle) % r) - (r / 2);
        }

        Debug.Log("Angle: " + angle + ", Direction: " + direction + ", head: " + calc);

        if (direction % 2 == 0) {
            head.rotation = Quaternion.Euler(0, 0, calc);
            float scale = myScale - Mathf.Abs(calc / 360f);
            head.localScale = new Vector3(scale, myScale, 1f);
            head.localPosition = new Vector3((calc / (360f * 4f)) - 0.073f, 0.45f, 0f);

            body.rotation = Quaternion.Euler(0, 0, calc / 8);
            scale = bodyScale - Mathf.Abs(calc / 360f);
            body.localScale = new Vector3(scale, bodyScale, 1f);
            body.localPosition = new Vector3((calc / (360f * 2f)), 0f, 0f);

            gun.rotation = Quaternion.Euler(0, 0, calc);
            scale = gunScale - Mathf.Abs(calc / 360f);
            gun.localScale = new Vector3(scale, gunScale, 1f);
            gun.localPosition = new Vector3((calc / (360f * 2f)), 0f, 0f);
        } else {
            head.rotation = Quaternion.Euler(0, 0, calc / 2);
            float scale = myScale - Mathf.Abs(calc / 360f) * 4f;
            head.localScale = new Vector3(scale, myScale, 1f);
            head.localPosition = new Vector3((calc / (360f * 4f)) - 0.073f, 0.45f, 0f);

            body.rotation = Quaternion.Euler(0, 0, calc / 4);
            scale = bodyScale - Mathf.Abs(calc / 360f) * 4f;
            body.localScale = new Vector3(scale, bodyScale, 1f);
            body.localPosition = new Vector3((calc / (360f * 2f)), 0f, 0f);

            gun.rotation = Quaternion.Euler(0, 0, 90 + (calc));
            scale = gunScale - Mathf.Abs(calc / 360f) * 4f;
            gun.localScale = new Vector3(gunScale, scale, 1f);
            gun.localPosition = new Vector3((calc / (360f * 4f)) - 0.32f, 0f, 0f);
        }

        switch (direction) {
            case 0: gunRenderer.sortingOrder = 6;
                gunRenderer.sprite = gunHorizontal;
                gun.localScale = new Vector3(gun.localScale.x, gun.localScale.y, gun.localScale.z);
                break;
            case 1: gunRenderer.sortingOrder = -1;
                gunRenderer.sprite = gunVertical;
                gun.localScale = new Vector3(gun.localScale.x, gun.localScale.y, gun.localScale.z);
                break;
            case 2: gunRenderer.sortingOrder = -1;
                gunRenderer.sprite = gunHorizontal;
                gun.localScale = new Vector3(gun.localScale.x * -1, gun.localScale.y, gun.localScale.z);
                break;
            case 3: gunRenderer.sortingOrder = 1;
                gunRenderer.sprite = gunVertical;
                gun.localScale = new Vector3(gun.localScale.x * -1, gun.localScale.y, gun.localScale.z);
                break;
            default: break;
        }
    }

    int GetMouseAngle() {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (Vector2)mouseWorldPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) { angle += 360f; }
        return ((int)angle);
    }
}
