using UnityEngine;

public static class LoopHelper
{
    // loops position in 2D world, assuming 0, 0 is center
    public static Vector3 LoopPos(Vector3 pos, int width, int height) {
        Vector3 newPos = new Vector3(pos.x%(width/2), pos.y%(height/2), 0);
        return newPos;
    }
}
