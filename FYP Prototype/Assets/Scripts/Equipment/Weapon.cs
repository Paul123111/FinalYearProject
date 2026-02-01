using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Equipment/Weapon")]
public class Weapon : ScriptableObject
{
    public Sprite weaponSprite;
    [SerializeField] int _firerate;
    public int firerate {
        get { return _firerate; }
    }

    public virtual bool UseWeapon(ProjectilePool pool, Transform t, bool isEnemy, AudioSource audio) {
        return false;
    }

    public Sprite GetSprite() {
        return weaponSprite;
    }
}
