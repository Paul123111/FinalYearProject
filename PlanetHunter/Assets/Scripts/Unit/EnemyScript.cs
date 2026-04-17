using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    
    [SerializeField] UnitStats stats;
    UnitMovement move;
    HealthSystem healthSystem;
    UseEquipment equip;
    Transform target;
    
    void Awake() {
        move = GetComponent<UnitMovement>();
        equip = GetComponent<UseEquipment>();
        healthSystem = GetComponentInChildren<HealthSystem>();
        move.SetStats(stats);
        healthSystem.SetStats(stats);

        target = GameObject.Find("Player").transform;
    }

    void Update() {
    }
}

