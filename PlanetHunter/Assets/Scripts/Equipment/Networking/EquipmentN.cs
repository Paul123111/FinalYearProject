using Mirror;
using System;
using UnityEngine;

public enum Type { Weapon, Armour, Helmet };
public enum EType { Gun, Body, Helmet };

public class EquipmentN : ScriptableObject {
    public EType type;
    [NonSerialized] public int id;
}

[CreateAssetMenu(fileName = "HelmetN", menuName = "EquipmentN/Head")]
public class Head : EquipmentN {
    public AnimatorOverrideController anim;
    public Head() {
        type = EType.Helmet;
    }
}

[CreateAssetMenu(fileName = "BodyN", menuName = "EquipmentN/Body")]
public class Body : EquipmentN {
    public AnimatorOverrideController anim;
    public Sprite hand;
    public Body() {
        type = EType.Body;
    }
}

[CreateAssetMenu(fileName = "WeaponN", menuName = "EquipmentN/Weapon")]
public class Gun : EquipmentN {
    public Sprite sprite;
    public Gun() {
        type = EType.Gun;
    }
}

