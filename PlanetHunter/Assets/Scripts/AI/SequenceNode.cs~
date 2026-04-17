using UnityEngine;

public class SequenceNode : MonoBehaviour, IRunnable
{
    [SerializeField] IRunnable[] _nodes;
    int _i = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public status Run() {
        status result = _nodes[_i].Run();
        if (result == status.SUCCESS) {
            if (_i+1 < _nodes.Length) {
                _i++;
            } else {
                return status.SUCCESS;
            }
        } else if (result == status.FAILURE) {
            return result;
        }
        return status.RUNNING;
    }
}
