using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileN", menuName = "EquipmentN/Projectile")]
public class ProjectilePropertiesN : ScriptableObject {
    [NonSerialized] public int id;

    [Header("Bullet Stats")]
    public int damage = 10;
    public bool destroyOnHit = true;
    public float speed = 20f;
    public float maxLifetime = 3f;

    [Header("Bullet Appearance")]
    public Sprite sprite;

    [Header("Bullet Size")]
    public float initScale = 0.1f; // bullet grows in size
    public float maxScale = 2;
    public float scaleRateOfChange = 2f;
}
