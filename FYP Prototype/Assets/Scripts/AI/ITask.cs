using UnityEngine;

public enum status{FAILURE, SUCCESS, RUNNING};

public interface INode
{
    // task is an interface with a Run() method to run the state
    status Run();
}
