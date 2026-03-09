using System.Linq;
using UnityEngine;

public class SequenceNode : INode
{
    [SerializeField] INode[] _nodes;
    int _i = 0;

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

    public INode AddNode<T>(T node) where T : INode
    {
        _nodes.Append(node);
        return node;
    }
}
