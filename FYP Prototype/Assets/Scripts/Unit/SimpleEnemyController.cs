using UnityEngine;
using System.Collections.Generic;

public class SimpleEnemyController : MonoBehaviour
{
    UnitMovement controller;
    UseEquipment weapon;
    Transform target;
    Transform player;
    
    [SerializeField] LayerMask layerMask;
    ContactFilter2D contactFilter;
    List<Collider2D> nearbyEquip;

    void Awake()
    {
        controller = GetComponent<UnitMovement>();
        weapon = GetComponent<UseEquipment>();
        target = GameObject.Find("Player").transform;
        player = GameObject.Find("Player").transform;
        nearbyEquip = new List<Collider2D>();
    }

    void Start() {
        contactFilter.SetLayerMask(layerMask);
    }

    // Update is called once per frame
    void Update()
    {
        //if ((player.position-transform.position).magnitude < 10f && weapon.weapon.GetSprite() != null) {
        //    target = GameObject.Find("Player").transform;
        //    controller.SetTarget(target.position);
        //    weapon.SetTarget(target.position);
        //    RunFromTarget();
        //    Chase();
        //} else if ((player.position-transform.position).magnitude < 10f) {
        //    target = GameObject.Find("Player").transform;
        //    controller.SetTarget(target.position);
        //    weapon.SetTarget(target.position);
        //    MoveTowardsTarget();
        //} else if ((FindClosestEquipment()?.gameObject.transform.position-transform.position)?.magnitude < 30f) {
        //    target = FindClosestEquipment()?.gameObject?.transform;
        //    controller.SetTarget(target.position);
        //    weapon.SetTarget(target.position);
        //    MoveTowardsTarget();
        //} else {
        //    controller.SetTarget(player.position);
        //    weapon.SetTarget(player.position);
        //    RunFromTarget();
        //}
        controller.SetTarget(target.position);
        weapon.SetTarget(target.position);
        RunFromTarget();
        Chase();
        MoveTowardsTarget();
    }

    void MoveTowardsTarget() {
        Vector3 dir = target.position-transform.position;
        dir = Vector3.Normalize(dir);
        controller.SetDirection(dir);
    }
    
    void RunFromTarget() {
        Vector3 dir = target.position-transform.position;
        dir = Vector3.Normalize(dir);
        controller.SetDirection(-dir);
    }
    
    void Chase() { 
        if ((target.position-transform.position).magnitude < 10f) {
            weapon.SetAttacking(1);
        } else {
            weapon.SetAttacking(0);
        }
    }
    
    Collider2D? FindClosestEquipment() {
        int numEnemies = Physics2D.OverlapCircle(transform.position, 30f, contactFilter, nearbyEquip);
        Debug.Log(numEnemies);
        if (numEnemies == 0) return null;
        // float.MaxValue used so that first collider distance is definitely lower than initial value
        Vector3 distance = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Collider2D? pick = null;
        foreach (Collider2D c in nearbyEquip) {
            Vector3 newDistance = c.transform.position - transform.position;
            if (newDistance.magnitude < distance.magnitude) {
                distance = newDistance;
                pick = c;
            }
        }
        return pick;
    }
}

