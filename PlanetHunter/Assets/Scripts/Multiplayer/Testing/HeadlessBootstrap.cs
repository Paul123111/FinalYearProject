using System.Threading.Tasks;
using UnityEngine;

public class HeadlessBootstrap : MonoBehaviour
{
    [SerializeField] ServerApi serverApi;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        if (Application.isBatchMode) {
            await Task.Delay(5000);
            serverApi.ButtonAllocate();
        } else {
            Destroy(gameObject);
        }
    }
}
