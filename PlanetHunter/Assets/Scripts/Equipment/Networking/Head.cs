using Items;
using UnityEngine;

namespace Items {
    [CreateAssetMenu(fileName = "HelmetN", menuName = "EquipmentN/Head")]
    public class Head : EquipmentN {
        public AnimatorOverrideController anim;
        public Head() {
            type = EType.Helmet;
        }
    }
}
