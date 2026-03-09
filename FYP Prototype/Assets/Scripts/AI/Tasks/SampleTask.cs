using UnityEngine;

public class SampleTask : MonoBehaviour, INode
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public status Run() {
        Debug.Log("Sample Task");
        return status.SUCCESS;
    }
}
