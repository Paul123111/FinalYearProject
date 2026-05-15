using Mirror;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSelect : NetworkBehaviour
{
    [SerializeField] GameObject outline;

    bool nearbyLocal = false;
    [SyncVar(hook = nameof(OnPlayerNearby))] bool nearby = false;

    HashSet<GameObject> playersNearby = new HashSet<GameObject>();

    //[Server]
    //bool PlayersNearby() {
    //    foreach (var conn in NetworkServer.connections.Values) {
    //        if (conn.identity != null) {
    //            GameObject player = conn.identity.gameObject;
    //            if (Vector3.Distance(player.transform.position, transform.position) <= radius) {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    void OnPlayerNearby(bool oldValue, bool newValue) {
        outline.SetActive(newValue);
    }

    //private void Update() {
    //    if (!NetworkServer.active) return;
    //    bool n = PlayersNearby();
    //    if (n != nearby) {
    //        nearby = n;
    //    }
    //}

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D other) {
        NetworkIdentity identity = other.GetComponent<NetworkIdentity>();
        Debug.Log(identity);
        Debug.Log(identity?.gameObject);
        Debug.Log(identity?.gameObject?.tag);
        if (identity?.gameObject?.tag == "Player") {
            playersNearby.Add(identity.gameObject);
            nearbyLocal = playersNearby.Count > 0;
            if (nearbyLocal != nearby) {
                nearby = nearbyLocal;
            }
        }
    }

    [ServerCallback]
    private void OnTriggerExit2D(Collider2D other) {
        NetworkIdentity identity = other.GetComponent<NetworkIdentity>();
        if (identity?.gameObject?.tag == "Player") {
            playersNearby.Remove(identity.gameObject);
            nearbyLocal = playersNearby.Count > 0;
            if (nearbyLocal != nearby) {
                nearby = nearbyLocal;
            }
        }
    }
}
