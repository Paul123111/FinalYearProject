using Mirror;
using ProcGen;
using UnityEngine;

namespace Items {
    public class EquipmentSlots : NetworkBehaviour {
        [SerializeField] private EquipmentDatabase database;
        [SerializeField] bool randomise;
        Head _head;
        Body _body;
        Gun _gun;

        [Header("Initial Slots")]
        public Head initHead;
        public Body initBody;
        public Gun initGun;

        public Head head {
            get => _head;
            set { _head = value; if (isServer) {syncHeadId = value != null ? value.id : 0; RefreshAll(); } }
        }
        public Body body {
            get => _body;
            set { _body = value; if (isServer) {syncBodyId = value != null ? value.id : 0; RefreshAll(); } }
        }
        public Gun gun {
            get => _gun;
            set { _gun = value; if (isServer) { syncGunId = value != null ? value.id : 0; RefreshAll(); } }
        }

        [SyncVar(hook = nameof(OnSlotChanged))] private int syncHeadId = 0;
        [SyncVar(hook = nameof(OnSlotChanged))] private int syncBodyId = 0;
        [SyncVar(hook = nameof(OnSlotChanged))] private int syncGunId = 0;

        private void OnSlotChanged(int oldId, int newId) {
            RefreshAll();
        }

        [Command]
        public void SetHead(int headId) {
            if (database != null) {
                this.head = database.GetHeadByIndex(headId);
            }
        }

        [Command]
        public void SetBody(int bodyId) {
            if (database != null) {
                this.body = database.GetBodyByIndex(bodyId);
            }
        }

        [Command]
        public void SetGun(int gunId) {
            if (database != null) {
                this.gun = database.GetGunByIndex(gunId);
            }
        }

        // pub sub system
        public System.Action OnEquipmentChanged;
        public void RefreshAll() {
            if (database != null) {
                Debug.Log(syncBodyId);
                _head = database.GetHeadByIndex(syncHeadId);
                _body = database.GetBodyByIndex(syncBodyId);
                _gun = database.GetGunByIndex(syncGunId);
            }
            OnEquipmentChanged?.Invoke();
        }

        public override void OnStartServer() {
            if (database != null) database.Initialize();


            if (randomise) {
                int rand = Mathf.Abs(gameObject.GetEntityId()) + System.DateTime.Now.Millisecond;
                if (rand == 0) rand = 100;
                syncHeadId = ProcGenLib.PseudoRandomRange(1, database.heads.Length, rand, out rand);
                syncBodyId = ProcGenLib.PseudoRandomRange(0, database.bodies.Length, rand, out rand);
                syncGunId = ProcGenLib.PseudoRandomRange(0, database.guns.Length, rand, out rand);
            } else {
                syncHeadId = initHead != null ? initHead.id : 0;
                syncBodyId = initBody != null ? initBody.id : 0;
                syncGunId = initGun != null ? initGun.id : 0;
            }
            //RefreshAll();
        }

        public override void OnStartClient() {
            if (database != null) database.Initialize();
            RefreshAll();
        }
    }
}
