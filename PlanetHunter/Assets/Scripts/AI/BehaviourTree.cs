using UnityEngine;

public class BehaviourTree : MonoBehaviour
{
    [SerializeField] Node currentNode;

    void Tick() {
        status result = currentNode.Run();
    }

    public void SetInitialNode<T>(T node) where T : Node {
        currentNode = node;
    }
}
