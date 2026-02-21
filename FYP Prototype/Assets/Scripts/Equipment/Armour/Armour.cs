using UnityEngine;

[CreateAssetMenu(fileName = "Armour", menuName = "Equipment/Armour")]
public class Armour : Equipment
{
    [SerializeField] float _damageResistanceBonus;
    [SerializeField] int _maxHealthBonus;
    [SerializeField] float _evasionBonus;
    [SerializeField] float _speedMultiplierBonus;
    [SerializeField] float _regenBonus;
    [SerializeField] float _damageMultiplierBonus;
    public Color color;
    
    public Armour() {
        this._type = Type.Armour;
    }

    public float damageResistanceBonus { get {return _damageResistanceBonus;} }
    public int maxHealthBonus { get {return _maxHealthBonus;} }
    public float evasionBonus { get {return _evasionBonus;} }
    public float speedMultiplierBonus { get {return _speedMultiplierBonus;} }
    public float regenBonus { get {return _regenBonus;} }
    public float damageMultiplierBonus { get {return _damageMultiplierBonus;} }
}
