using UnityEngine;

public class ProjectilePool : PoolController2D<ProjectileScript>
{
    public void SpawnProjectile(Transform t, bool isEnemy, ProjectileProperties props) {
        var itemObject = SpawnOne();
        itemObject.Setup(_pool, t, isEnemy, props);
        itemObject.ActivateAt(t.position);
    }
}
