using Mirror;
using UnityEngine;

public enum PlayerType { ROCKET, ASTRONAUT };

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] GameObject astronaut;
    [SerializeField] GameObject rocket;

    [SyncVar(hook = nameof(SwitchPlayer))] PlayerType type;

    public void SwitchPlayer(PlayerType old, PlayerType t) {
        if (t == PlayerType.ROCKET) {
            transform.position = Vector3.zero;
            astronaut.SetActive(false);
            rocket.SetActive(true);
        } else {
            transform.position = Vector3.zero;
            astronaut.SetActive(true);
            rocket.SetActive(false);
        }
    }

    public void SetType(PlayerType t) {
        type = t;
    }
}
