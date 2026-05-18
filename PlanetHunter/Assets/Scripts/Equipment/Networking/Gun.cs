using UnityEngine;

namespace Items {

    [CreateAssetMenu(fileName = "WeaponN", menuName = "EquipmentN/Weapon")]
    public class Gun : EquipmentN {
        public Sprite sprite;
        public Gun() {
            type = EType.Gun;
        }
    }
}
