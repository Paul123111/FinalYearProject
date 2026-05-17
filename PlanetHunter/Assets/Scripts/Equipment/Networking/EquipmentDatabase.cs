using UnityEngine;

namespace Items {
    [CreateAssetMenu(fileName = "EquipmentDatabase", menuName = "EquipmentN/Database")]
    public class EquipmentDatabase : ScriptableObject {
        public Head[] heads;
        public Body[] bodies;
        public Gun[] guns;

        public void Initialize() {
            for (int i = 0; i < heads.Length; i++) {
                if (heads[i] != null) heads[i].id = i;
            }
            for (int i = 0; i < bodies.Length; i++) {
                if (bodies[i] != null) bodies[i].id = i;
            }
            for (int i = 0; i < guns.Length; i++) {
                if (guns[i] != null) guns[i].id = i;
            }
            Debug.Log("Equipment Database Indices Initialized!");
        }

        public Head GetHeadByIndex(int index) => (index >= 0 && index < heads.Length) ? heads[index] : null;
        public Body GetBodyByIndex(int index) => (index >= 0 && index < bodies.Length) ? bodies[index] : null;
        public Gun GetGunByIndex(int index) => (index >= 0 && index < guns.Length) ? guns[index] : null;
    }
}