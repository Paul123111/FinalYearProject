using UnityEngine;

[CreateAssetMenu(fileName = "RangedWeapon", menuName = "Equipment/RangedWeapon")]
public class RangedWeapon : Weapon
{ 
    [SerializeField] ProjectileProperties props;

    public override bool UseWeapon(ProjectilePool pool, Transform t, bool isEnemy, AudioSource audio) { 
        pool.SpawnProjectile(t, isEnemy, props);
        audio.Play();
        return true;
    }
}
