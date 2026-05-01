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
    float myScale = 1f;
    float bodyScale = 1.5f;

    int headR = 120;
    int bodyR = 60;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anims = GetComponentsInChildren<Animator>();
    }

    void Update() {
        angle = GetMouseAngle();
        //direction = ((angle + 45) / 90) % 4;
        if (angle >= 270-(bodyR/2) && angle < 270+(bodyR/2)) {
            direction = 3;
        } else if (angle >= 90-(bodyR/2) && angle < 90+(bodyR/2)) {
            direction = 1;
        } else if (angle >= 270+(bodyR/2) || angle < 90-(bodyR/2)) {
            direction = 0;
        } else {
            direction = 2;
        }
        anims[0].SetInteger("Direction", direction);
        anims[1].SetInteger("Direction", direction);
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
            head.rotation = Quaternion.Euler(0, 0, calc / 4);
            float scale = myScale - Mathf.Abs(calc / 360f);
            head.localScale = new Vector3(scale, myScale, 1f);
            head.localPosition = new Vector3((calc / (360f * 4f)) - 0.073f, 0.45f, 0f);

            body.rotation = Quaternion.Euler(0, 0, calc / 8);
            scale = bodyScale - Mathf.Abs(calc / 360f);
            body.localScale = new Vector3(scale, bodyScale, 1f);
            body.localPosition = new Vector3((calc / (360f * 2f)), 0f, 0f);
        } else {
            head.rotation = Quaternion.Euler(0, 0, calc / 2);
            float scale = myScale - Mathf.Abs(calc / 360f) * 4f;
            head.localScale = new Vector3(scale, myScale, 1f);
            head.localPosition = new Vector3((calc / (360f * 4f)) - 0.073f, 0.45f, 0f);

            body.rotation = Quaternion.Euler(0, 0, calc / 4);
            scale = bodyScale - Mathf.Abs(calc / 360f) * 4f;
            body.localScale = new Vector3(scale, bodyScale, 1f);
            body.localPosition = new Vector3((calc / (360f * 2f)), 0f, 0f);
        }

    }

    // Update is called once per frame
    //void Update()
    //{
    //    angle = GetMouseAngle();
    //    direction = ((angle+45)/90)%4;
    //    if (angle >= 270 && angle <= 285) {
    //        direction = 1;
    //    } else if (angle >= 75 && angle <= 105) {
    //        direction = 3;
    //    } else if (angle >= 255 && angle <= 75) {
    //        direction = 2;
    //    } else {
    //        direction = 0;
    //    }
    //    anims[0].SetInteger("Direction", direction);
    //    anims[1].SetInteger("Direction", direction);
    //    Debug.Log("Angle: " + angle + ", Direction: " + direction);
    //    if (direction % 2 == 0) {
    //        //head.rotation = Quaternion.Euler(0, 0, ((angle + 45) % 90) - 45);
    //        //head.localScale = new Vector3(myScale, myScale, 1f);
    //        head.rotation = Quaternion.Euler(0, 0, (((angle + 45) % 90) - 45) / 4);
    //        float scale = myScale - Mathf.Abs((((angle + 45) % 90) - 45) / 360f);
    //        head.localScale = new Vector3(scale, myScale, 1f);
    //        head.localPosition = new Vector3(((((angle + 45) % 90) - 45) / (360f * 4f)) - 0.073f, 0.45f, 0f);
    //    } else {
    //        head.rotation = Quaternion.Euler(0, 0, (((angle + 45) % 90) - 45)/4);
    //        float scale = myScale - Mathf.Abs((((angle + 45) % 90) - 45) / 360f);
    //        head.localScale  = new Vector3(scale, myScale, 1f);
    //        head.localPosition = new Vector3(((((angle + 45) % 90) - 45) / (360f*4f))-0.073f, 0.45f, 0f);
    //    }

    //    if (direction % 2 == 0) {
    //        //head.rotation = Quaternion.Euler(0, 0, ((angle + 45) % 90) - 45);
    //        //head.localScale = new Vector3(myScale, myScale, 1f);
    //        body.rotation = Quaternion.Euler(0, 0, (((angle + 45) % 90) - 45) / 4);
    //        float scale = bodyScale - Mathf.Abs((((angle + 45) % 90) - 45) / 360f);
    //        body.localScale = new Vector3(scale, bodyScale, 1f);
    //        body.localPosition = new Vector3(((((angle + 45) % 90) - 45) / (360f * 1f)), 0f, 0f);
    //    } else {
    //        body.rotation = Quaternion.Euler(0, 0, (((angle + 45) % 90) - 45) / 8);
    //        float scale = bodyScale - Mathf.Abs((((angle + 45) % 90) - 45) / 360f);
    //        body.localScale = new Vector3(scale, bodyScale, 1f);
    //        body.localPosition = new Vector3(((((angle + 45) % 90) - 45) / (360f * 2f)), 0f, 0f);
    //    }

    //}

    int GetMouseAngle() {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (Vector2)mouseWorldPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) { angle += 360f; }
        return ((int)angle);
    }
}
