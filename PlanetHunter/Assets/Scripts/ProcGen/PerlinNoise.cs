using UnityEngine;
using UnityEngine.Tilemaps;

public class PerlinNoise : MonoBehaviour
{
    [SerializeField] Tilemap t;
    [SerializeField] Tile t1;
    [SerializeField] Tile t2;
    [SerializeField] Tile t3;
    [SerializeField] Tile t4;
    float timer = 0.5f;
    int[,] colors;
    bool fullMap = false;
    int steps = 0;

    [SerializeField] int worldWidth = 100;
    [SerializeField] int worldHeight = 100;

    void Start()
    {
        InitTiles();
    }
    
    void Update()
    {
        if (fullMap) return;
        timer -= Time.deltaTime;
        if (timer <= 0) {
            UpdateTiles();
            timer = 0.0f;
        }
    }

    public void InitTiles() {
        steps = 0;
        colors = ChooseTiles(worldWidth, worldHeight, (int)System.DateTime.Now.Ticks);
        fullMap = false;
        PlaceTiles();
    }

    void PlaceTiles() {
        for (int w = 0; w < colors.GetLength(0); w++) {
            for (int h = 0; h < colors.GetLength(1); h++) {
                switch(colors[w,h]) {
                    case 0: t.SetTile(new Vector3Int(w-(worldWidth/2), h-(worldHeight/2), 0), t1); break;
                    case 1: t.SetTile(new Vector3Int(w-(worldWidth/2), h-(worldHeight/2), 0), t2); break;
                    case 2: t.SetTile(new Vector3Int(w-(worldWidth/2), h-(worldHeight/2), 0), t3); break;
                    case 3: t.SetTile(new Vector3Int(w-(worldWidth/2), h-(worldHeight/2), 0), t4); break;
                }
            }
        }
    }

    void UpdateTiles() {
        colors = MyGameStep(colors);
        fullMap = FullMap(colors);
        PlaceTiles();
    }

    // generate random number - not using random as it creates problems between editor and different types of builds
    // min inclusive, max exclusive
    public static int PseudoRandomRange(int min, int max, int seed) {
        if (min >= max) return min;
        uint uSeed = (uint)seed;
        uSeed = (1103515245 * uSeed + 12345);
        uint rand = (uSeed / 65536) % 32768;
        int range = max - min;
        return min + ((int)rand % (range));
    }

    // pick tiles based on noise
    int[,] ChooseTiles(int worldWidth, int worldHeight, int seed) {
        float[,] noise = GenerateNoise(worldWidth, worldHeight, seed);
        int[,] tiles = new int[worldWidth, worldHeight];
        for (int w = 0; w < tiles.GetLength(0); w++) {
            for (int h = 0; h < tiles.GetLength(1); h++) {
                if (noise[w,h] < 15f) {
                    tiles[w,h] = 0;
                } else { 
                    tiles[w,h] = PseudoRandomRange(10, 40, seed)/10;
                }
            }
        }
        return tiles;
    }

    static public float[,] GenerateNoise(int worldWidth, int worldHeight, int seed) {
        // 2d array of 2d vectors in corners
        Vector2[,] grid = new Vector2[worldWidth+1, worldHeight+1];

        // setting up vectors at corners
        for (int w = 0; w < grid.GetLength(0); w++) {
            for (int h = 0; h < grid.GetLength(1); h++) {
                grid[w,h] = new Vector2(w+PseudoRandomRange(-1, 1, seed), h+PseudoRandomRange(-1, 1, seed));
            }
        }

        // get coordinates of candidate points in grid 
        Vector2[,] points = new Vector2[worldWidth, worldHeight];
        for (int w = 0; w < points.GetLength(0); w++) {
            for (int h = 0; h < points.GetLength(1); h++) {
                points[w,h] = new Vector2(w+0.5f, h+0.5f);
            }
        }

        // get dot product of distance and gradient vectors
        float[,,] dotProd = new float[worldWidth, worldHeight, 4];
        for (int w = 0; w < points.GetLength(0); w++) {
            for (int h = 0; h < points.GetLength(1); h++) {
                dotProd[w,h,0] = Vector2.Dot(points[w,h]-grid[w,h], grid[w,h]);
                dotProd[w,h,1] = Vector2.Dot(points[w,h]-grid[w+1,h], grid[w+1,h]);
                dotProd[w,h,2] = Vector2.Dot(points[w,h]-grid[w,h+1], grid[w,h+1]);
                dotProd[w,h,3] = Vector2.Dot(points[w,h]-grid[w+1,h+1], grid[w+1,h+1]);
            }
        }

        // linearly interpolate dot products
        float[,] lerp1 = new float[worldWidth, worldHeight];
        float[,] lerp2 = new float[worldWidth, worldHeight];
        float[,] lerp3 = new float[worldWidth, worldHeight];
        for (int w = 0; w < points.GetLength(0); w++) {
            for (int h = 0; h < points.GetLength(1); h++) {
                lerp1[w,h] = Mathf.Lerp(dotProd[w,h,0], dotProd[w,h,1], 0.5f);
                lerp2[w,h] = Mathf.Lerp(dotProd[w,h,2], dotProd[w,h,3], 0.5f);
                lerp3[w,h] = Mathf.Lerp(lerp1[w,h], lerp2[w,h], 0.5f);
            }
        }
        return lerp3;

    }

    // cellular automata currently using Conway's game of life as a test
    // will change rules to change terrain generation
    public int[,] ConwayGameStep(int[,] tiles) {
        for (int w = 0; w < tiles.GetLength(0); w++) {
            for (int h = 0; h < tiles.GetLength(1); h++) {
                int neighbours = GetNumNeighbours(tiles, w, h, 1);
                if (neighbours < 2) {tiles[w,h] = 0;}
                else if (neighbours > 4) {tiles[w,h] = 0;}
                else if (neighbours == 3) {tiles[w,h] = 1;}
            }
        }
        return tiles;
    }

    [SerializeField] int minNeighbours = 2;
    [SerializeField] int minTypeNeighbours = 2;
    [SerializeField] int maxNeighbours = 4;
    [SerializeField] int createNeighbours = 3;

    public int[,] MyGameStep(int[,] tiles) {
        steps++;
        // anti infinite
        if (steps > 50) {
            for (int w = 0; w < tiles.GetLength(0); w++) {
                for (int h = 0; h < tiles.GetLength(1); h++) {
                    int type = tiles[w,h];
                    if (type == 0) {
                        tiles[w,h] = 1;
                    }
                }
            }
            Debug.Log("Stopped infinite");
            return tiles;
        }

        // step based on rules below
        bool veryEmpty = PercentEmpty(tiles) > 0.9f;
        for (int w = 0; w < tiles.GetLength(0); w++) {
            for (int h = 0; h < tiles.GetLength(1); h++) {
                int type = tiles[w,h];
                int typeNeighbours = GetNumNeighbours(tiles, w, h, type);
                int empty = GetNumNeighbours(tiles, w, h, 0);
                int grass = GetNumNeighbours(tiles, w, h, 1);
                int water = GetNumNeighbours(tiles, w, h, 2);
                int red = GetNumNeighbours(tiles, w, h, 3);
                int neighbours = grass+water+red;
                if (neighbours < minNeighbours) {tiles[w,h] = 0;}
                else if (neighbours > maxNeighbours) {tiles[w,h] = 0;}
                else if ((neighbours > createNeighbours && type == 0) || (neighbours > 1 && veryEmpty && type == 0)) 
                {tiles[w,h] = grass >= water ? (grass > red ? 1 : 3) : (water >= red ? 2 : 3);}
                else if (typeNeighbours < minTypeNeighbours) {tiles[w,h] = 0;}
            }
        }
        return tiles;
    }

    int GetNumNeighbours(int[,] tiles, int x, int y, int type) { 
        int neighbours = 0;
        for (int w = x-1; w <= x+1; w++) {
            for (int h = y-1; h <= y+1; h++) {
                // dont count self
                //if (w < 0 || h < 0 || w >= tiles.GetLength(0) || h >= tiles.GetLength(1)) continue;
                if (w == x && h == y) continue;
                if (tiles[Mathf.Abs(w)%(tiles.GetLength(0)), Mathf.Abs(h)%(tiles.GetLength(1))] == type) neighbours++;
            }
        }
        return neighbours;
    }

    bool FullMap(int[,] tiles) { 
        for (int w = 0; w < tiles.GetLength(0); w++) {
            for (int h = 0; h < tiles.GetLength(1); h++) {
                if (tiles[w,h] == 0) return false;
            }
        }
        Debug.Log("Generation Complete");
        return true;
    }
    
    float PercentEmpty(int[,] tiles) {
        int empty = 0;
        int map = 0;
        for (int w = 0; w < tiles.GetLength(0); w++) {
            for (int h = 0; h < tiles.GetLength(1); h++) {
                if (tiles[w,h] == 0) empty++;
                map++;
            }
        }
        return (float) empty/map;
    }

}

