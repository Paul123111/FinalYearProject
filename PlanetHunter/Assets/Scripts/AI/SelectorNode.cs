using System;
using System.Linq;

public class SelectorNode : INode
{
    INode[] nodes;
    int index = 0;

    public status Run()
    {
        status childResult = nodes[index].Run();
        if (childResult == status.RUNNING) {
            return status.RUNNING;
        } else if (childResult == status.SUCCESS) {
            index = 0;
            return status.SUCCESS;
        } else {
            index++;
            if (index >= nodes.Length) {
                return status.FAILURE;
            }
            return status.RUNNING;
        }
    }

    public INode AddNode<T>(T node) where T : INode
    {
        nodes.Append(node);
        return node;
    }
}
