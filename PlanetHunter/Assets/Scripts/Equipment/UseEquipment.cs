using UnityEngine;
using UnityEngine.UI;

public class UseEquipment : MonoBehaviour
{

    [SerializeField] Weapon currentWeapon;
    [SerializeField] SpriteRenderer currentWeaponRenderer;
    [SerializeField] SpriteRenderer unitRenderer;
    [SerializeField] SpriteRenderer headRenderer;
    [SerializeField] Transform spawn;
    [SerializeField] Image uiImage;
    [SerializeField] Image uiImageArmour;
    [SerializeField] Image uiImageHelmet;
    Vector3 targetPos = new Vector3(0, 0);
    ProjectilePool pool;
    float cooldown = 0;
    float attacking = 0;
    [SerializeField] bool isEnemy;
    AudioSource audio;

    // temporary
    UnitStats stats;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentWeaponRenderer.sprite = currentWeapon.GetSprite();
        pool = GameObject.Find("ProjectilePool").GetComponent<ProjectilePool>();
        audio = GameObject.Find("WeaponSound").GetComponent<AudioSource>();
        stats = GetComponent<UnitMovement>().stats;
    }

    // Update is called once per frame
    void Update()
    {
        cooldown -= Time.deltaTime;
        LookAtTarget();
        if (cooldown <= 0 && attacking > 0) {
            cooldown = 1.0f/currentWeapon.firerate;
            currentWeapon.UseWeapon(pool, spawn, isEnemy, audio);
        }
    }

    public void SetAttacking(float a) {
        attacking = a;
    }

    public void SetWeapon(Weapon weapon) {
        currentWeapon = weapon;
        currentWeaponRenderer.sprite = currentWeapon.GetSprite();
        if (uiImage != null) {
            uiImage.sprite = currentWeapon.GetSprite();
        }
    }

    public Weapon weapon {
        get { return currentWeapon; }
    }
    
    void LookAtTarget() {
        Vector3 newPos = targetPos;
        newPos -= transform.position;
        float angle = Mathf.Atan2(newPos.y, newPos.x) * Mathf.Rad2Deg;
        spawn.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
    
    public void SetTarget(Vector3 target) {
        targetPos = target;
    }

    public void SetArmour(Armour armour) {
        stats.armour = armour;
        unitRenderer.color = armour.color;
        if (uiImageArmour != null) {
            uiImageArmour.sprite = armour.GetSprite();
        }
    }
    public void SetHelmet(Helmet armour) {
        //stats.helmet = armour;
        headRenderer.color = armour.color;
        if (uiImageHelmet != null) {
            uiImageHelmet.sprite = armour.GetSprite();
        }
    }
}

