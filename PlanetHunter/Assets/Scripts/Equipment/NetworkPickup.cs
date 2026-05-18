using Items;
using Mirror;
using UnityEngine;

public class NetworkPickup : NetworkBehaviour {
    [SerializeField] EquipmentN equipment;
    SpriteRenderer spriteRenderer;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (equipment != null) {
            spriteRenderer.sprite = equipment.pickupSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!isServer) return;

        EquipmentSlots slots = other.GetComponent<EquipmentSlots>();
        if (slots != null) {
            if (equipment.type == EType.Helmet) {
                slots.head = equipment as Head;
            } else if (equipment.type == EType.Body) {
                slots.body = equipment as Body;
            } else if (equipment.type == EType.Gun) {
                slots.gun = equipment as Gun;
            }
            NetworkServer.Destroy(gameObject);
        }
    }
}
