using Agones;
using Mirror;
using UnityEngine;

#if UNITY_EDITOR
using ParrelSync;
#endif

[RequireComponent(typeof(AgonesAlphaSdk))]
public class AgonesServer : MonoBehaviour {
        
    private AgonesAlphaSdk agones;

#if UNITY_EDITOR || !UNITY_SERVER
    void Awake() {
        if (!ClonesManager.IsClone()) {
            Destroy(gameObject);
        }
    }
#endif

    async void Start() {
        agones = GetComponent<AgonesAlphaSdk>();
        bool ok = await agones.Connect();
        if (ok) {
            Debug.Log(("Server - Connected"));
        } else {
            Debug.Log(("Server - Failed to connect, exiting"));
            Application.Quit(1);
        }

        ok = await agones.Ready();
        await agones.SetPlayerCapacity(4);
        if (ok) {
            Debug.Log($"Server - Ready");
        } else {
            Debug.Log($"Server - Ready failed");
            Application.Quit();
        }

        NetworkManager.singleton.StartServer();
    }
}
