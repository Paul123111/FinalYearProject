using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAnchor : MonoBehaviour {
    public Camera cam;
    private Transform playerTransform;
    public float maxLookDistance = 6f;

    public void Initialise(Transform player) {
        playerTransform = player;
    }

    void Update() {
        MoveCursor();
    }

    void MoveCursor() {
        if (cam == null || playerTransform == null) return;

        // get local cursor world space
        Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0f;

        // clamp the distance relative to the local player so the camera doesn't fly off-screen
        Vector3 direction = mousePos - playerTransform.position;
        if (direction.magnitude > maxLookDistance) {
            transform.position = playerTransform.position + direction.normalized * maxLookDistance;
        } else {
            transform.position = mousePos;
        }
    }
}
