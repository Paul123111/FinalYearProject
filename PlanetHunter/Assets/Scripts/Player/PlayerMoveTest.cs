using UnityEngine;
using UnityEngine.InputSystem; // Required for the new system

public class PlayerMoveTest : MonoBehaviour {
    public float moveSpeed = 5f;

    private Rigidbody2D body;
    private Vector2 moveInput;

    void Awake() {
        body = GetComponent<Rigidbody2D>();
    }
    void OnMove(InputValue value) {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate() {
        body.MovePosition(body.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}