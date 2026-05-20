using Mirror;
using Unity.Cinemachine;
using UnityEngine;

public class NetworkPlayerCamera : NetworkBehaviour
{
    private CinemachineCamera virtualCamera;

    private void Awake() {
        virtualCamera = GetComponent<CinemachineCamera>();

        if (virtualCamera != null) {
            virtualCamera.enabled = false;
        }
    }

    public override void OnStartLocalPlayer() {
        if (virtualCamera != null) {
            virtualCamera.enabled = true;
            virtualCamera.Target.TrackingTarget = transform.parent;
        }
        MouseAnchor mouseAnchor = FindAnyObjectByType<MouseAnchor>();
        mouseAnchor.Initialise(transform.parent);
    }
}
