using UnityEngine;


public enum status{FAILURE, SUCCESS, RUNNING};

public interface IRunnable
{
    // task is an interface with a Run() method to run the state
    status Run();
}

