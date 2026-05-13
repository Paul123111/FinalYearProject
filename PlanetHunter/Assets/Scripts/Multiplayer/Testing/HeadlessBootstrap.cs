using System.Threading.Tasks;
using UnityEngine;

public class HeadlessBootstrap : MonoBehaviour
{
    ServerApiUI serverApiUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        if (Application.isBatchMode) {
            await Task.Delay(5000);
            serverApiUI = FindFirstObjectByType<ServerApiUI>();
            serverApiUI.Allocate();
        } else {
            Destroy(gameObject);
        }
    }
}
