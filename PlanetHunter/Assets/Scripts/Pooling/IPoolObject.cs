using UnityEngine;

public interface IPoolObject
{ 
    public void ActivateAt(Vector3 worldPos); 
    public void ReturnToPool();
}

