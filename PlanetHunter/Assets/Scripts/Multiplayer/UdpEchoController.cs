using UnityEngine;
using Mirror;
#if UNITY_EDITOR
    using ParrelSync;
#endif

namespace AgonesExample
{
    public class UdpEchoController : MonoBehaviour
    {
        [SerializeField]
        private GameObject server;
        [SerializeField]
        private GameObject client;

        void Awake()
        {
            server.SetActive(false);
            client.SetActive(false);

#if UNITY_EDITOR
            if (ClonesManager.IsClone()) {
                server.SetActive(true);
            } else {
                client.SetActive(true);
            }
#endif

#if UNITY_SERVER && !UNITY_EDITOR
            server.SetActive(true);
#elif !UNITY_EDITOR
            client.SetActive(true);
#endif

        }

#if UNITY_EDITOR
        void Start() {
            if (ClonesManager.IsClone()) {
                NetworkManager.singleton.StartServer();
            }
        }
#endif
    }
}