using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSelect : NetworkBehaviour
{
    [SerializeField] GameObject outline;

    bool nearbyLocal = false;
    [SyncVar(hook = nameof(OnPlayerNearby))] bool nearby = false;
    public bool selected { get { return nearby; } } 

    HashSet<GameObject> playersNearby = new HashSet<GameObject>();

    void OnPlayerNearby(bool oldValue, bool newValue) {
        outline.SetActive(newValue);
        NetworkClient.localPlayer.GetComponent<NetworkPlayer>().VoteForLevel(this, !newValue);
    }

    [Server]
    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Player")) return;
        NetworkIdentity identity = other.GetComponent<NetworkIdentity>();
        if (identity != null) {
            if (!playersNearby.Add(identity.gameObject)) return;
            nearbyLocal = playersNearby.Count > 0;
            if (nearbyLocal != nearby) {
                nearby = nearbyLocal;
            }
        }
    }

    [Server]
    private void OnTriggerExit2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Player")) return;
        NetworkIdentity identity = other.GetComponent<NetworkIdentity>();
        if (identity != null) {
            if (!playersNearby.Remove(identity.gameObject)) return;
            nearbyLocal = playersNearby.Count > 0;
            if (nearbyLocal != nearby) {
                nearby = nearbyLocal;
            }
        }
    }
}
