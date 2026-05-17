using Mirror;
using UnityEngine;

namespace Items {
    public class EquipmentSlots : NetworkBehaviour {
        [SerializeField] private EquipmentDatabase database;
        Head _head;
        Body _body;
        Gun _gun;

        public Head head {
            get => _head;
            set { _head = value; if (isServer) { syncHeadId = value != null ? value.id : 0; } }
        }
        public Body body {
            get => _body;
            set { _body = value; if (isServer) { syncBodyId = value != null ? value.id : 0; } }
        }
        public Gun gun {
            get => _gun;
            set { _gun = value; if (isServer) { syncGunId = value != null ? value.id : 0; } }
        }

        [SyncVar(hook = nameof(OnSlotChanged))] private int syncHeadId = 0;
        [SyncVar(hook = nameof(OnSlotChanged))] private int syncBodyId = 0;
        [SyncVar(hook = nameof(OnSlotChanged))] private int syncGunId = 0;

        private void OnSlotChanged(int oldId, int newId) {
            RefreshAll();
        }

        // pub sub system
        public System.Action OnEquipmentChanged;
        public void RefreshAll() {
            if (database != null) {
                _head = database.GetHeadByIndex(syncHeadId);
                _body = database.GetBodyByIndex(syncBodyId);
                _gun = database.GetGunByIndex(syncGunId);
            }
            OnEquipmentChanged?.Invoke();
        }

        public override void OnStartServer() {
            if (database != null) database.Initialize();
            RefreshAll();
        }

        public override void OnStartClient() {
            if (database != null) database.Initialize();
            RefreshAll();
        }
    }
}
