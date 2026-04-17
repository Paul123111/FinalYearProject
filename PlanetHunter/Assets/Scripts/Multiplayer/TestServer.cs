using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Agones;

[RequireComponent(typeof(AgonesSdk2))]
public class TestServer : MonoBehaviour
{
    private int Port { get; set; } = 7777;
    private UdpClient client = null;
    private AgonesSdk2 agones = null;

    async void Start() {
        client = new UdpClient(Port);

        agones = GetComponent<AgonesSdk2>();
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
