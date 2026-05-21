using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class HeadlessBootstrap : MonoBehaviour
{
    ServerApiUI serverApiUI;
    ServerApiBackend serverApiBackend;
    private Keyboard virtualKeyboard;
    private Mouse virtualMouse;
    aiShoot[] enemies;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        if (Application.isBatchMode) {
            DontDestroyOnLoad(gameObject);
            await Task.Delay(5000);

            serverApiUI = FindAnyObjectByType<ServerApiUI>();
            serverApiBackend = FindAnyObjectByType<ServerApiBackend>();
            bool joined = false;

            string jsonResponse = await serverApiBackend.ListRooms();
            GameServerResponse[] serversJson = JsonHelper.ParseArray<GameServerResponse>(jsonResponse);
            foreach (var server in serversJson) {
                if (server.players < 3) {
                    serverApiUI.JoinServer(server);
                    joined = true;
                    break;
                }
            }
            if (!joined) serverApiUI.Allocate();
            await Task.Delay(5000);

            virtualKeyboard = Keyboard.current;
            virtualMouse = Mouse.current;

            if (virtualKeyboard == null) {
                virtualKeyboard = InputSystem.GetDevice<Keyboard>() ?? InputSystem.AddDevice<Keyboard>();
                InputSystem.EnableDevice(virtualKeyboard);
            }
            if (virtualMouse == null) {
                virtualMouse = InputSystem.GetDevice<Mouse>() ?? InputSystem.AddDevice<Mouse>();
                InputSystem.EnableDevice(virtualMouse);
            }

            InputUser.PerformPairingWithDevice(virtualKeyboard);
            InputUser.PerformPairingWithDevice(virtualMouse);
            StartCoroutine(SimulateRandomInputRoutine());
        } else {
            Destroy(gameObject);
        }
    }

    System.Collections.IEnumerator SimulateRandomInputRoutine() {
        while (true) {
            // get random key
            Key[] keys = { Key.W, Key.A, Key.S, Key.D };
            Key randomKey = keys[Random.Range(0, keys.Length)];

            MouseState releaseStateM;
            virtualMouse.CopyState(out releaseStateM);
            Vector2 targetScreenPos = new Vector2(Random.Range(-50,50), Random.Range(-50, 50));
            releaseStateM.delta = targetScreenPos;
            releaseStateM.WithButton(MouseButton.Left, true);

            KeyboardState pressState;
            virtualKeyboard.CopyState(out pressState);
            pressState.Set(randomKey, true);
            InputSystem.QueueStateEvent(virtualKeyboard, pressState);
            InputSystem.QueueStateEvent(virtualMouse, releaseStateM);
            Debug.Log($"Simulated Press: {randomKey}");
            InputSystem.Update();

            yield return new WaitForSeconds(0.2f);

            KeyboardState releaseState;
            virtualKeyboard.CopyState(out releaseState);
            releaseState.Set(randomKey, false);

            MouseState pressStateM = new MouseState();
            targetScreenPos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            pressStateM.delta = Vector2.zero;
            pressStateM.WithButton(MouseButton.Left, false);

            InputSystem.QueueStateEvent(virtualKeyboard, releaseState);
            InputSystem.QueueStateEvent(virtualMouse, pressStateM);
            InputSystem.Update();


            yield return new WaitForSeconds(0.1f);

            //MouseState pressState = new MouseState();
            //pressState.position = targetScreenPos;
            //pressState.WithButton(MouseButton.Left, true);

            //InputSystem.QueueDeltaStateEvent(virtualKeyboard[randomKey], 0f);
            //InputSystem.QueueStateEvent(virtualMouse, pressState);
            //Debug.Log($"Simulated Release: {randomKey}");
        }
    }

    Transform GetClosestEnemy() {
        enemies = FindObjectsByType<aiShoot>();
        float maxDist = 1000000;
        float dist = maxDist;
        Transform closest = null;
        foreach (var enemy in enemies) {
            dist = (enemy.transform.position - transform.position).sqrMagnitude;
            if (dist < maxDist) {
                maxDist = dist;
                closest = enemy.transform;
            }
        }
        return closest;
    }
}
