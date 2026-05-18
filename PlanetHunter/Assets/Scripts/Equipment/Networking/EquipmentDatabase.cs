using ProcGen;
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

        public EquipmentN GetRandom(int seed) {
            int rand = Mathf.Abs(seed);
            if (rand == 0) rand = 100;
            int type = ProcGenLib.PseudoRandomRange(0, 3, rand, out rand);
            if (type == 0) {
                return GetHeadByIndex(ProcGenLib.PseudoRandomRange(0, heads.Length, rand, out rand));
            } else if (type == 1) {
                return GetBodyByIndex(ProcGenLib.PseudoRandomRange(0, bodies.Length, rand, out rand));
            } else if (type == 2) {
                return GetGunByIndex(ProcGenLib.PseudoRandomRange(0, guns.Length, rand, out rand));
            }
            return null;
        }
    }
}