using UnityEngine;
using Mirror;
using TMPro;

public class WinCondition : NetworkBehaviour
{
    [SyncVar(hook = nameof(UpdateUI))] int count = 0;
    [SyncVar] int maxCount = 10;
    [SerializeField] TextMeshProUGUI counter;

    public static WinCondition Instance { get; private set; }
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    [Server]
    public void IncCount() {
        count++;
        if (count >= maxCount) {
            PlanetHunterNetworkManager.singleton.TravelToSpace("GameNetworking");
        }
    }

    public void UpdateUI(int old, int newV) {
        counter.text = $"Enemies Remaining: {maxCount - newV}";
    }

}
