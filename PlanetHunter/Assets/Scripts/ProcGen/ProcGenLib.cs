using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProcGen {
    public static class ProcGenLib {

        // pick tiles based on noise
        public static int[,] ChooseTiles(int worldWidth, int worldHeight, int numTypes, float noiseThreshold, int seed) {
            float[,] noise = GenerateNoise(worldWidth, worldHeight, seed);
            int[,] tiles = new int[worldWidth, worldHeight];
            int rand = seed;
            for (int w = 0; w < tiles.GetLength(0); w++) {
                for (int h = 0; h < tiles.GetLength(1); h++) {
                    if (noise[w, h] < noiseThreshold) {
                        tiles[w, h] = 0;
                    } else {
                        tiles[w, h] = PseudoRandomRange(10, 10*numTypes, rand, out rand) / 10;
                    }
                }
            }
            return tiles;
        }

        // generate random number - not using random as it creates problems between editor and different types of builds
        // min inclusive, max exclusive
        public static int PseudoRandomRange(int min, int max, int seed, out int randState) {
            randState = 0;
            if (min >= max) return min;
            uint uSeed = (uint)seed;
            uSeed = (1103515245 * uSeed + 12345);
            uint rand = (uSeed / 65536) % 32768;

            randState = (int)rand;
            int range = max - min;
            return min + ((int)rand % (range));
        }
        public static float PseudoRandomRangeF(float min, float max, int seed, out int randState) {
            randState = 0;
            if (min >= max) return min;
            uint uSeed = (uint)seed;
            uSeed = (1103515245 * uSeed + 12345);
            uint rand = (uSeed / 65536) % 32768;
            randState = (int)rand;
            float percent = (float)rand / 32768f;
            return min + (percent * (max - min));
        }

        public static float[,] GenerateNoise(int worldWidth, int worldHeight, int seed) {
            // 2d array of 2d vectors in corners
            Vector2[,] grid = new Vector2[worldWidth + 1, worldHeight + 1];
            int rand = seed;
            // setting up vectors at corners
            for (int w = 0; w < grid.GetLength(0); w++) {
                for (int h = 0; h < grid.GetLength(1); h++) {
                    float angle = PseudoRandomRangeF(0f, Mathf.PI * 2f, rand, out rand);
                    grid[w, h] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                }
            }

            // get coordinates of candidate points in grid 
            Vector2[,] points = new Vector2[worldWidth, worldHeight];
            for (int w = 0; w < points.GetLength(0); w++) {
                for (int h = 0; h < points.GetLength(1); h++) {
                    points[w, h] = new Vector2(w * 0.1f, h * 0.1f);
                }
            }

            // get dot product of distance and gradient vectors
            float[,,] dotProd = new float[worldWidth, worldHeight, 4];
            for (int w = 0; w < points.GetLength(0); w++) {
                for (int h = 0; h < points.GetLength(1); h++) {
                    Vector2 p = points[w, h];

                    int x0 = Mathf.FloorToInt(p.x);
                    int x1 = x0 + 1;
                    int y0 = Mathf.FloorToInt(p.y);
                    int y1 = y0 + 1;

                    // calculate point distance from corners
                    Vector2 dist1 = new Vector2(p.x-x0, p.y-y0);
                    Vector2 dist2 = new Vector2(p.x-x1, p.y-y0);
                    Vector2 dist3 = new Vector2(p.x-x0, p.y-y1);
                    Vector2 dist4 = new Vector2(p.x-x1, p.y-y1);

                    int gX0 = x0 % worldWidth;
                    int gX1 = x1 % worldWidth;
                    int gY0 = y0 % worldHeight;
                    int gY1 = y1 % worldHeight;

                    dotProd[w, h, 0] = Vector2.Dot(dist1, grid[gX0, gY0]);
                    dotProd[w, h, 1] = Vector2.Dot(dist2, grid[gX1, gY0]);
                    dotProd[w, h, 2] = Vector2.Dot(dist3, grid[gX0, gY1]);
                    dotProd[w, h, 3] = Vector2.Dot(dist4, grid[gX1, gY1]);
                }
            }

            // linearly interpolate dot products
            float[,] lerp1 = new float[worldWidth, worldHeight];
            float[,] lerp2 = new float[worldWidth, worldHeight];
            float[,] lerp3 = new float[worldWidth, worldHeight];
            for (int w = 0; w < points.GetLength(0); w++) {
                for (int h = 0; h < points.GetLength(1); h++) {
                    Vector2 p = points[w, h];

                    float weightX = p.x - Mathf.Floor(p.x);
                    float weightY = p.y - Mathf.Floor(p.y);

                    float u = weightX * weightX * weightX * (weightX * (weightX * 6f - 15f) + 10f);
                    float v = weightY * weightY * weightY * (weightY * (weightY * 6f - 15f) + 10f);

                    lerp1[w, h] = Mathf.Lerp(dotProd[w, h, 0], dotProd[w, h, 1], u);
                    lerp2[w, h] = Mathf.Lerp(dotProd[w, h, 2], dotProd[w, h, 3], u);
                    lerp3[w, h] = Mathf.Lerp(lerp1[w, h], lerp2[w, h], v);
                    lerp3[w, h] = Mathf.InverseLerp(-0.5f, 0.5f, lerp3[w,h]);
                }
            }
            return lerp3;

        }

        // legacy: cellular automata currently using Conway's game of life as a test
        // will change rules to change terrain generation
        public static int[,] ConwayGameStep(int[,] tiles) {
            for (int w = 0; w < tiles.GetLength(0); w++) {
                for (int h = 0; h < tiles.GetLength(1); h++) {
                    int neighbours = GetNumNeighbours(tiles, w, h, 1);
                    if (neighbours < 2) { tiles[w, h] = 0; } else if (neighbours > 4) { tiles[w, h] = 0; } else if (neighbours == 3) { tiles[w, h] = 1; }
                }
            }
            return tiles;
        }

        public static int[,] PlanetStep(int[,] tiles, int numTypes, int gen, int maxGens,
            int minNeighbours, int minTypeNeighbours, int maxNeighbours, int createNeighbours) {
            if (numTypes <= 0) return null;

            // anti infinite
            if (gen > maxGens) {
                for (int w = 0; w < tiles.GetLength(0); w++) {
                    for (int h = 0; h < tiles.GetLength(1); h++) {
                        int type = tiles[w, h];
                        if (type == 0) {
                            tiles[w, h] = 1;
                        }
                    }
                }
                Debug.Log("Stopped infinite generation");
                return tiles;
            }

            // step based on rules below
            bool veryEmpty = PercentEmpty(tiles) > 0.95f;
            int[] individualNeighbours = new int[numTypes - 1];
            int[,] readTarget = (int[,])tiles.Clone();

            for (int w = 0; w < tiles.GetLength(0); w++) {
                for (int h = 0; h < tiles.GetLength(1); h++) {
                    // get type of current tile
                    int type = tiles[w, h];
                    // neighbour values
                    int typeNeighbours = GetNumNeighbours(readTarget, w, h, type);
                    int neighbours = 0;
                    int maxCountNeighbours = -1;
                    int popularType = -1;

                    // important: first tile in array is the empty tile
                    for (int i = 0; i < numTypes - 1; i++) {
                        individualNeighbours[i] = GetNumNeighbours(readTarget, w, h, i + 1);
                        neighbours += individualNeighbours[i];
                        // get most popular non-empty neighbour type
                        if (individualNeighbours[i] > maxCountNeighbours) {
                            popularType = i + 1;
                            maxCountNeighbours = individualNeighbours[i];
                        }
                    }

                    if (neighbours < minNeighbours) {
                        tiles[w, h] = 0;
                    } else if (neighbours > maxNeighbours) {
                        tiles[w, h] = 0;
                    } else if ((neighbours > createNeighbours && type == 0) || (neighbours > 1 && veryEmpty && type == 0)) {
                        tiles[w, h] = popularType;
                    } else if (type != 0 && typeNeighbours < minTypeNeighbours) {
                        tiles[w, h] = 0;
                    }
                }
            }
            return tiles;
        }

        // Get specific amount of neighbours of type
        public static int GetNumNeighbours(int[,] tiles, int x, int y, int type) {
            int neighbours = 0;
            for (int w = x - 1; w <= x + 1; w++) {
                for (int h = y - 1; h <= y + 1; h++) {
                    // dont count self
                    //if (w < 0 || h < 0 || w >= tiles.GetLength(0) || h >= tiles.GetLength(1)) continue;
                    if (w == x && h == y) continue;
                    if (tiles[Mod(w, tiles.GetLength(0)), Mod(h, tiles.GetLength(1))] == type) neighbours++;
                }
            }
            return neighbours;
        }

        static int Mod(int x, int m) {
            if (m <= 0) return -999;
            return (x % m + m) % m;
        }

        public static bool FullMap(int[,] tiles) {
            for (int w = 0; w < tiles.GetLength(0); w++) {
                for (int h = 0; h < tiles.GetLength(1); h++) {
                    if (tiles[w, h] == 0) return false;
                }
            }
            Debug.Log("Full Map!");
            return true;
        }

        public static float PercentEmpty(int[,] tiles) {
            int empty = 0;
            int map = 0;
            for (int w = 0; w < tiles.GetLength(0); w++) {
                for (int h = 0; h < tiles.GetLength(1); h++) {
                    if (tiles[w, h] == 0) empty++;
                    map++;
                }
            }
            return (float)empty / map;
        }

        // checks for enclosed areas using Breadth First Search
        public static bool EnclosedAreasPresent(int[,] walls) {
            // 0 on walls is walkable ground - find first walkable tile and
            // see if it can reach every non-wall tile on the map
            Vector2Int root = new Vector2Int(-1, -1);
            for (int x = 0; x < walls.GetLength(0); x++) {
                for (int y = 0; y < walls.GetLength(1); y++) {
                    if (walls[x, y] == 0) {
                        root = new Vector2Int(x, y);
                        break;
                    }
                }
                if (root.x != -1) break;
            }
            if (root.x == -1 || root.y == -1) return true;
            Vector2Int[] remainingTiles = WallBFS(walls, root);
            return remainingTiles.Length != 0;
        }

        public static Vector2Int[] WallBFS(int[,] walls, Vector2Int root) {
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(root);
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            visited.Add(root);
            int x = -1;
            int y = -1;
            Vector2Int i;

            // search tiles around root until no more walkable tiles can be explored
            while (queue.Count > 0) {
                Vector2Int currentTile = queue.Dequeue();

                x = Mod(currentTile.x, walls.GetLength(0));
                y = Mod(currentTile.y, walls.GetLength(1));

                walls[x, y] = 2;
                // check left and right tiles
                for (int a = -1; a <= 1; a += 2) {
                    int tx = Mod(x - a, walls.GetLength(0));
                    int ty = Mod(y, walls.GetLength(1));
                    i = new Vector2Int(tx, ty);
                    if (!visited.Contains(i) && walls[tx, ty] == 0) {
                        walls[tx, ty] = 2;
                        queue.Enqueue(i);
                        visited.Add(i);
                    }
                }
                // check top and bottom tiles
                for (int a = -1; a <= 1; a += 2) {
                    int tx = Mod(x, walls.GetLength(0));
                    int ty = Mod(y - a, walls.GetLength(1));
                    i = new Vector2Int(tx, ty);
                    if (!visited.Contains(i) && walls[tx, ty] == 0) {
                        walls[tx, ty] = 2;
                        queue.Enqueue(i);
                        visited.Add(i);
                    }
                }
            }

            List<Vector2Int> remainingTiles = new List<Vector2Int>();
            for (int j = 0; j < walls.GetLength(0); j++) {
                for (int k = 0; k < walls.GetLength(1); k++) {
                    if (walls[j, k] == 0) {
                        i = new Vector2Int(j, k);
                        remainingTiles.Add(i);
                    }
                }
            }
            return remainingTiles.ToArray();
        }

        public static Vector3Int GetRandomSpawn(int seed, Tilemap wall, Tilemap separate) {
            int rand = seed;
            int loopSafety = 0; // ensure infinite loop does not happen
            Vector3Int spawnPoint = new Vector3Int(PseudoRandomRange(0, 99, rand, out rand), PseudoRandomRange(0, 99, rand, out rand));
            if (separate != null) {
                while (wall.GetTile(spawnPoint) != null && separate.GetTile(spawnPoint) != null
                 && loopSafety < 50) { // check ground exists
                    loopSafety++;
                    spawnPoint = new Vector3Int(PseudoRandomRange(0, 99, rand, out rand), PseudoRandomRange(0, 99, rand, out rand));
                }
            } else {
                while (wall.GetTile(spawnPoint) != null
                    && loopSafety < 50) { // check ground exists
                    loopSafety++;
                    spawnPoint = new Vector3Int(PseudoRandomRange(0, 99, rand, out rand), PseudoRandomRange(0, 99, rand, out rand));
                }
            }
            return spawnPoint;
        }
    }
}
