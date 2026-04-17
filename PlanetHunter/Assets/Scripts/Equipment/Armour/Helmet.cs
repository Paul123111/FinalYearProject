using UnityEngine;

[CreateAssetMenu(fileName = "Helmet", menuName = "Equipment/Helmet")]
public class Helmet : Equipment
{ 
    public Color color;

    public Helmet() {
        this._type = Type.Helmet;
    }
}
