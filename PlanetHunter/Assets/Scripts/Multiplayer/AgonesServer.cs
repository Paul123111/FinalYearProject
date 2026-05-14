using Agones;
using Mirror;
using UnityEngine;

#if UNITY_EDITOR
using ParrelSync;
#endif

[RequireComponent(typeof(AgonesBetaSdk))]
public class AgonesServer : MonoBehaviour {
        
    private AgonesBetaSdk agones;

    void Awake() {
#if UNITY_EDITOR
        if (!ClonesManager.IsClone()) {
            Destroy(gameObject);
        }
#elif !UNITY_SERVER
        Destroy(gameObject);
#endif

    }

    async void Start() {
        DontDestroyOnLoad(gameObject);
        agones = GetComponent<AgonesBetaSdk>();
        bool ok = await agones.Connect();
        if (ok) {
            Debug.Log(("Server - Connected"));
        } else {
            Debug.Log(("Server - Failed to connect, exiting"));
            Application.Quit(1);
        }

        ok = await agones.Ready();
        if (ok) {
            Debug.Log($"Server - Ready");
        } else {
            Debug.Log($"Server - Ready failed");
            Application.Quit();
        }

        NetworkManager.singleton.StartServer();
    }
}
