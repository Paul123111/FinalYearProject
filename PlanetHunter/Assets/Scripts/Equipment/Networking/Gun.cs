using UnityEngine;

namespace Items {

    [CreateAssetMenu(fileName = "WeaponN", menuName = "EquipmentN/Weapon")]
    public class Gun : EquipmentN {
        public Sprite sprite;
        public ProjectilePropertiesN props;
        public float cooldown;
        public Gun() {
            type = EType.Gun;
        }
    }
}
