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
        contactFilter.useTriggers = true;
    }

    // Update is called once per frame
    void Update()
    {
        if ((player.position-transform.position).magnitude < 4f && weapon.weapon.GetSprite() != null) {
            target = GameObject.Find("Player").transform;
            controller.SetTarget(target.position);
            weapon.SetTarget(target.position);
            RunFromTarget();
            Chase();
        } else if ((player.position-transform.position).magnitude < 10f) {
            target = GameObject.Find("Player").transform;
            controller.SetTarget(target.position);
            weapon.SetTarget(target.position);
            MoveTowardsTarget();
        } else if ((FindClosestEquipment()?.gameObject.transform.position-transform.position)?.magnitude < 60f) {
            target = FindClosestEquipment()?.gameObject?.transform;
            controller.SetTarget(target.position);
            weapon.SetTarget(target.position);
            MoveTowardsTarget();
        } else {
            controller.SetTarget(transform.position);
            weapon.SetTarget(player.position);
            MoveTowardsTarget();
        }
        //controller.SetTarget(target.position);
        //weapon.SetTarget(target.position);
        //RunFromTarget();
        //Chase();
        //MoveTowardsTarget();
    }

    void MoveTowardsTarget() {
        if (target == null) return;
        Vector3 dir = target.position-transform.position;
        if (Mathf.Abs(dir.x) > 50f) {
            dir.x *= -1;
        }
        if (Mathf.Abs(dir.y) > 50f) {
            dir.y *= -1;
        }
        dir = Vector3.Normalize(dir);
        controller.SetDirection(dir);
    }
    
    void RunFromTarget() {
        Vector3 dir = target.position-transform.position;
        dir = Vector3.Normalize(dir);
        controller.SetDirection(-dir);
    }
    
    void Chase() { 
        if ((target.position-transform.position).magnitude < 4f) {
            weapon.SetAttacking(1);
        } else {
            weapon.SetAttacking(0);
        }
    }
    
    Collider2D? FindClosestEquipment() {
        int numEnemies = Physics2D.OverlapCircle(transform.position, 60f, contactFilter, nearbyEquip);
        //Debug.Log(numEnemies);
        if (numEnemies == 0) return null;
        // float.MaxValue used so that first collider distance is definitely lower than initial value
        Vector3 distance = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Collider2D? pick = null;
        foreach (Collider2D c in nearbyEquip) {
            Debug.Log(c.name);
            Vector3 newDistance = c.transform.position - transform.position;
            if (newDistance.magnitude < distance.magnitude) {
                distance = newDistance;
                pick = c;
            }
        }
        return pick;
    }
}

