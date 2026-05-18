using Items;
using UnityEngine;

namespace Items {
    [CreateAssetMenu(fileName = "BodyN", menuName = "EquipmentN/Body")]
    public class Body : EquipmentN {
        public AnimatorOverrideController anim;
        public Sprite hand;
        public Body() {
            type = EType.Body;
        }
    }
}
