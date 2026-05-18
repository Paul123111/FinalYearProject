using Mirror;
using System;
using UnityEngine;
public enum Type { Weapon, Armour, Helmet };

namespace Items {

    public enum EType { Gun, Body, Helmet };

    public class EquipmentN : ScriptableObject {
        public EType type;
        public Sprite pickupSprite;
        [NonSerialized] public int id;
    }

}

