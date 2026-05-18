public enum status{FAILURE, SUCCESS, RUNNING};

public interface INode {
    status Run();
}

public abstract class Node : INode {
    // run is called every tick
    public abstract status Run();
}
