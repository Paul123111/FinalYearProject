using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] float playerSpeed = 1f;
    [SerializeField] Transform playerBody;
    Vector3 inputValue = new Vector3(0, 0);
    Camera camera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    { 
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position += inputValue * playerSpeed * Time.deltaTime;
        LookAtMouse();
    }
    
    void OnMove(InputValue value) {
        Vector2 moveValue = value.Get<Vector2>();
        inputValue = new Vector2(moveValue.x, moveValue.y);
    }

    void LookAtMouse() {
        Vector2 mouse_pos = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouse_pos.x = mouse_pos.x-gameObject.transform.position.x;
        mouse_pos.y = mouse_pos.y-gameObject.transform.position.y;
        float angle = Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg;
        playerBody.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
    
    void OnDebugReset() {
        Debug.Log("Debug reset");
        PerlinNoise p = GameObject.Find("PerlinNoise").GetComponent<PerlinNoise>();
        p.InitTiles();
    }
}
