using System.Linq;
using UnityEngine;

public class BehaviourTree : MonoBehaviour
{
    INode initialNode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Run();
    }

    void Run() {
        status result = initialNode.Run();
    }

    public void SetInitialNode<T>(T node) where T : INode {
        initialNode = node;
    }
}
