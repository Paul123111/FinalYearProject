using UnityEngine;

public class PickupHitbox : MonoBehaviour
{
    
    [SerializeField] Equipment equipment;

    void Start()
    {
       GetComponent<SpriteRenderer>().sprite = equipment.GetSprite(); 
    }

    void OnTriggerEnter2D(Collider2D col) {
        UseEquipment useEquipment = col.gameObject.transform.parent.gameObject.GetComponent<UseEquipment>();
        if (useEquipment == null) {
            return;
        }
        Debug.Log(equipment?.GetType());
        if (equipment?.GetType() == Type.Weapon) {
            Weapon weapon = (Weapon) equipment;
            useEquipment.SetWeapon(weapon);
            Destroy(gameObject);
        } else if (equipment?.GetType() == Type.Armour) { 
            Armour armour = (Armour) equipment;
            useEquipment.SetArmour(armour);
            Destroy(gameObject);
        } else if (equipment?.GetType() == Type.Helmet) { 
            Helmet armour = (Helmet) equipment;
            useEquipment.SetHelmet(armour);
            Destroy(gameObject);
        }
        
    }

}
