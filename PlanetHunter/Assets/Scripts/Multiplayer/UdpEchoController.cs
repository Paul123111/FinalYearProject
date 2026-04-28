using Mirror;
using ParrelSync;
using UnityEngine;

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

            if (ClonesManager.IsClone()) {
                server.SetActive(true);
            } else {
                client.SetActive(true);
            }

//#if UNITY_SERVER
//            server.SetActive(true);
//#endif
//#if !UNITY_SERVER
//            client.SetActive(true);
//#endif
        }
    }
}