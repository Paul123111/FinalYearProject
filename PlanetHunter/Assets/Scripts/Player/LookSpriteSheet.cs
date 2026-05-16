using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class LookSpriteSheet : NetworkBehaviour
{

    int angle;
    int direction; // 0-3 based on diagonal quadrants: -45 to 45 is first, etc.
    Animator[] anims;
    [SerializeField] Transform head;
    [SerializeField] Transform body;
    [SerializeField] Transform gun;
    [SerializeField] Transform hand;
    [SerializeField] SpriteRenderer gunRenderer;
    [SerializeField] SpriteRenderer handRenderer;
    float myScale = 1f;
    float bodyScale = 1.5f;
    float gunScale = 1f;

    int headR = 120;
    int bodyR = 60;

    [SerializeField] bool dynamicRotation = true;

    public float handRot = 1f;
    public float handScale = 1f;
    public float handPos = 2f;
    public float handX = -0.258f;
    public float handY = 0.015f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anims = GetComponentsInChildren<Animator>();
    }

    void Update() {
        if (!isLocalPlayer) return;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 directionMouse = (Vector2)mouseWorldPos - (Vector2)transform.position;
        if (directionMouse.x > 90 || directionMouse.y > 90) {
            return;
        }
        angle = GetMouseAngle(directionMouse);
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

        if (direction % 2 == 0) {
            head.rotation = Quaternion.Euler(0, 0, calc / 2);
            float scale = myScale - Mathf.Abs(calc / 360f);
            head.localScale = new Vector3(scale, myScale, 1f);
            head.localPosition = new Vector3((calc / (360f * 4f)) - 0.073f, 0.45f, 0f);

            body.rotation = Quaternion.Euler(0, 0, calc / 8);
            scale = bodyScale - Mathf.Abs(calc / 360f);
            body.localScale = new Vector3(scale, bodyScale, 1f);
            body.localPosition = new Vector3((calc / (360f * 2f)), 0f, 0f);

            hand.rotation = Quaternion.Euler(0, 0, (calc / handRot) - (90 * (direction-1)));
            scale = bodyScale - Mathf.Abs(calc / 360f) * handScale;
            hand.localScale = new Vector3(scale, bodyScale, 1f);
            hand.localPosition = new Vector3(((calc / 360f) * handPos) + (handX * (direction - 1)), handY, 0f);

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

            hand.rotation = Quaternion.Euler(0, 0, calc / 4);
            scale = bodyScale - Mathf.Abs(calc / 360f) * 4f;
            hand.localScale = new Vector3(scale, bodyScale, 1f);
            hand.localPosition = new Vector3((calc / (360f * 2f)) - 0.32f, 0.1f, 0f);

            gun.rotation = Quaternion.Euler(0, 0, 90 + (calc));
            scale = gunScale/1.2f - Mathf.Abs(calc / 180f);
            gun.localScale = new Vector3(gunScale, scale, 1f);
            gun.localPosition = new Vector3((calc / (360f * 4f)) - 0.32f, 0f, 0f);
        }

        switch (direction) {
            case 0: gunRenderer.sortingOrder = 6;
                handRenderer.sortingOrder = 7;
                gun.localScale = new Vector3(gun.localScale.x, gun.localScale.y, gun.localScale.z);
                break;
            case 1: gunRenderer.sortingOrder = -3;
                handRenderer.sortingOrder = -1;
                gun.localScale = new Vector3(gun.localScale.x, gun.localScale.y, gun.localScale.z);
                break;
            case 2: gunRenderer.sortingOrder = -1;
                handRenderer.sortingOrder = -2;
                gun.localScale = new Vector3(gun.localScale.x * -1, gun.localScale.y, gun.localScale.z);
                break;
            case 3: gunRenderer.sortingOrder = 1;
                handRenderer.sortingOrder = -1;
                gun.localScale = new Vector3(gun.localScale.x * -1, gun.localScale.y, gun.localScale.z);
                break;
            default: break;
        }
    }

    int GetMouseAngle(Vector2 direction) {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) { angle += 360f; }
        return ((int)angle);
    }
}
