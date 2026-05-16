using Mirror;
using UnityEngine;

public class LoopHelper : NetworkBehaviour
{
    // loops position in 2D world, assuming 0, 0 is center - legacy
    public static Vector3 LoopPos(Vector3 pos, int width, int height) {
        if (pos.x > width/2) {
            pos.x -= width;
        } else if (pos.x < -width/2) {
            pos.x += width;
        }
        if (pos.y > height/2) {
            pos.y -= height;
        } else if (pos.y < -height/2) {
            pos.y += height;
        }
        Vector3 newPos = new Vector3(pos.x%(width/2), pos.y%(height/2), 0);
        return newPos;
    }

    NetworkTransformReliable nt;
    [SerializeField] float width = 100;
    [SerializeField] float height = 100;
    private float lastTeleportTime = 0;
    private const float TeleportCooldown = 0.15f;

    // loops position in 2D world, assuming 0, 0 is center
    public void LoopPosNetwork() {
        if (!isOwned && !isServer) return;
        if (Time.time - lastTeleportTime < TeleportCooldown) return;

        Vector3 pos = transform.position;
        bool looped = false;

        if (pos.x > width / 2f) {
            pos.x -= width;
            looped = true;
        } else if (pos.x < -width / 2f) {
            pos.x += width;
            looped = true;
        }
        if (pos.y > height / 2f) {
            pos.y -= height;
            looped = true;
        } else if (pos.y < -height / 2f) {
            pos.y += height;
            looped = true;
        }

        if (looped) {
            //pos = new Vector3(pos.x % (width / 2), pos.y % (height / 2), 0);
            lastTeleportTime = Time.time;
            ExecuteTeleport(pos);
        }
    }

    void Start() {
        nt = GetComponent<NetworkTransformReliable>();
    }

    private void Update() {
        LoopPosNetwork();
    }

    private void ExecuteTeleport(Vector3 targetPos) {
        if (isOwned) {
            transform.position = targetPos;
            nt.Reset();
            Loop(targetPos);
        } else if (isServer) {
            // if server-authoritative or unowned object running on server
            transform.position = targetPos;
            nt.Reset();
            RpcTeleportClients(targetPos);
        }
    }

    [Command]
    public void Loop(Vector3 pos) {
        transform.position = pos;
        nt.Reset();
        RpcTeleportClients(pos);
    }

    [ClientRpc(includeOwner = false)] // Owner already moved locally
    private void RpcTeleportClients(Vector3 targetPos) {
        transform.position = targetPos;
        nt.Reset();
    }
}
