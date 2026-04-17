using UnityEngine;

public class EnemyBehaviourTree : MonoBehaviour
{
    UnitMovement controller;
    UseEquipment weapon;
    Transform target;
    Transform player;

    void Awake()
    {
        controller = GetComponent<UnitMovement>();
        weapon = GetComponent<UseEquipment>();
        target = GameObject.Find("Player").transform;
        player = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        controller.SetTarget(target.position);
        weapon.SetTarget(target.position);
        MoveTowardsTarget();
    }

    void MoveTowardsTarget() {
        Vector3 dir = target.position-transform.position;
        dir = Vector3.Normalize(dir);
        controller.SetDirection(dir);
    }

    void Chase() { 
        if ((target.position-transform.position).magnitude < 10f) {
            weapon.SetAttacking(1);
        } else {
            weapon.SetAttacking(0);
        }
    }
}

