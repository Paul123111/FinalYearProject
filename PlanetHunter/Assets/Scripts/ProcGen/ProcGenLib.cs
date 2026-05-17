using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace ProcGen {
    public static class ProcGenLib {

        // pick tiles based on noise
        public static int[] ChooseTiles(int worldWidth, int worldHeight, int numTypes, float noiseThreshold, int seed) {
            float[] noise = GenerateNoise(worldWidth, worldHeight, seed);
            int[] tiles = new int[worldWidth * worldHeight];
            for (int i = 0; i < tiles.GetLength(0); i++) {
                if (noise[i] < noiseThreshold) {
                    tiles[i] = 0;
                } else {
                    tiles[i] = PseudoRandomRange(10, 10*numTypes, seed) / 10;
                }
            }
            return tiles;
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

        public static float[] GenerateNoise(int worldWidth, int worldHeight, int seed) {
            Random.InitState(seed);
            int gw = worldWidth + 1;
            int gh = worldHeight + 1;
            int w = worldWidth;
            int h = worldHeight;
            // 2d array of 2d vectors in corners
            Vector2[] grid = new Vector2[gw * gh];

            // setting up vectors at corners
            for (int i = 0; i < grid.Length; i++) {
                grid[i] = new Vector2(X(i, gw) + PseudoRandomRange(-1, 1, seed), Y(i, gh) + PseudoRandomRange(-1, 1, seed));
            }

            // get coordinates of candidate points in grid 
            Vector2[] points = new Vector2[w * h];
            for (int i = 0; i < points.Length; i++) {
                points[i] = new Vector2(X(i, w) + 0.5f, Y(i, h) + 0.5f);
            }

            // get dot product of distance and gradient vectors
            float[] dotProd = new float[w * h * 4];
            for (int i = 0; i < points.Length; i++) {
                dotProd[i] = Vector2.Dot(points[i] - grid[i], grid[i]);
                dotProd[i+1] = Vector2.Dot(points[i] - grid[i+1], grid[i+1]);
                dotProd[i+2] = Vector2.Dot(points[i] - grid[i+w], grid[i+w]);
                dotProd[i+3] = Vector2.Dot(points[i] - grid[i+w+1], grid[i+w+1]);
            }

            // linearly interpolate dot products
            float[] lerp1 = new float[w * h];
            float[] lerp2 = new float[w * h];
            float[] lerp3 = new float[w * h];
            for (int i = 0; i < points.Length; i++) {
                lerp1[i] = Mathf.Lerp(dotProd[i], dotProd[i], 0.5f);
                lerp2[i] = Mathf.Lerp(dotProd[i+2], dotProd[i+3], 0.5f);
                lerp3[i] = Mathf.Lerp(lerp1[i], lerp2[i], 0.5f);
            }
            return lerp3;

        }

        // legacy: cellular automata currently using Conway's game of life as a test
        // will change rules to change terrain generation
        //public static int[,] ConwayGameStep(int[,] tiles) {
        //    for (int w = 0; w < tiles.GetLength(0); w++) {
        //        for (int h = 0; h < tiles.GetLength(1); h++) {
        //            int neighbours = GetNumNeighbours(tiles, w, h, 1);
        //            if (neighbours < 2) { tiles[w, h] = 0; } else if (neighbours > 4) { tiles[w, h] = 0; } else if (neighbours == 3) { tiles[w, h] = 1; }
        //        }
        //    }
        //    return tiles;
        //}

        public static int[] PlanetStep(int[] tiles, int width, int numTypes, int gen, int maxGens,
            int minNeighbours, int minTypeNeighbours, int maxNeighbours, int createNeighbours) {
            if (numTypes <= 0) return null;

            // anti infinite
            if (gen > maxGens) {
                for (int i = 0; i < tiles.Length; i++) {
                    int type = tiles[i];
                    if (type == 0) {
                        tiles[i] = 1;
                    }
                }
                Debug.Log("Stopped infinite generation");
                return tiles;
            }

            // step based on rules below
            bool veryEmpty = PercentEmpty(tiles) > 0.95f;
            for (int i = 0; i < tiles.Length; i++) {
                // get type of current tile
                int type = tiles[i];
                // neighbour values
                int typeNeighbours = GetNumNeighbours(tiles, X(i, width), Y(i, width), width, type);
                int neighbours = 0;
                int popularType = -1;

                // important: first tile in array is the empty tile
                int[] individualNeighbours = new int[numTypes-1];
                for (int j = 0; j < numTypes-1; j++) {
                    individualNeighbours[j] = GetNumNeighbours(tiles, X(i, width), Y(i, width), width, j +1);
                    neighbours += individualNeighbours[j];
                    // get most popular non-empty neighbour type
                    if (individualNeighbours[j] > popularType) {
                        popularType = j+1;
                    }
                }

                if (neighbours < minNeighbours) {
                    tiles[i] = 0;
                } else if (neighbours > maxNeighbours) {
                    tiles[i] = 0; 
                } else if ((neighbours > createNeighbours && type == 0) || (neighbours > 1 && veryEmpty && type == 0)) {
                    tiles[i] = popularType;
                } else if (type != 0 && typeNeighbours < minTypeNeighbours) {
                    tiles[i] = 0;
                }
            }
            return tiles;
        }

        // Get specific amount of neighbours of type
        public static int GetNumNeighbours(int[] tiles, int x, int y, int width, int type) {
            int neighbours = 0;
            int[] dx = { -1,  0,  1, -1,  1, -1, 0, 1 };
            int[] dy = { -1, -1, -1,  0,  0,  1, 1, 1 };
            for (int i = 0; i < 8; i++) {
                int targetX = x + dx[i];
                int targetY = y + dy[i];

                int index = Mod(targetX, width) + (targetY * width);
                if (index < 0 || index >= tiles.Length) continue;
                if (tiles[index] == type) neighbours++;
            }
            return neighbours;
        }

        static int Mod(int x, int m) {
            if (m <= 0) return -999;
            return (x % m + m) % m;
        }

        public static bool FullMap(int[] tiles) {
            for (int i = 0; i < tiles.Length; i++) {
                if (tiles[i] == 0) return false;
            }
            Debug.Log("Full Map!");
            return true;
        }

        public static float PercentEmpty(int[] tiles) {
            int empty = 0;
            int map = 0;
            for (int i = 0; i < tiles.Length; i++) {
                if (tiles[i] == 0) empty++;
                map++;
            }
            return (float)empty / map;
        }

        // checks for enclosed areas using Breadth First Search
        //public static bool EnclosedAreasPresent(int[,] walls) {
        //    // 0 on walls is walkable ground - find first walkable tile and
        //    // see if it can reach every non-wall tile on the map
        //    Vector2Int root = new Vector2Int(-1, -1);
        //    for (int x = 0; x < walls.GetLength(0); x++) {
        //        for (int y = 0; x < walls.GetLength(0); x++) {
        //            if (walls[x, y] == 0) {
        //                root = new Vector2Int(x, y);
        //            }
        //        }
        //    }
        //    Vector2Int[] remainingTiles = WallBFS(walls, root);
        //    return remainingTiles.Length != 0;
        //}

        //public static Vector2Int[] WallBFS(int[,] walls, Vector2Int root) {
        //    List<Vector2Int> queue = new List<Vector2Int>();
        //    queue.Add(root);
        //    int x = -1;
        //    int y = -1;
        //    Vector2Int i;

        //    // search tiles around root until no more walkable tiles can be explored
        //    while (queue.Count > 0) {
        //        x = Mod(queue[0].x, walls.GetLength(0));
        //        y = Mod(queue[0].y, walls.GetLength(1));

        //        walls[x, y] = 2;
        //        for (int a = -1; a <= 1; a+=2) {
        //            x = Mod(queue[0].x-a, walls.GetLength(0));
        //            y = Mod(queue[0].y, walls.GetLength(1));
        //            if (walls[x, y] == 0) {
        //                walls[x, y] = 2;
        //                i = new Vector2Int(x, y);
        //                queue.Add(i);
        //            }
        //        }
        //        for (int a = -1; a <= 1; a += 2) {
        //            x = Mod(queue[0].x, walls.GetLength(0));
        //            y = Mod(queue[0].y-a, walls.GetLength(1));
        //            if (walls[x, y] == 0) {
        //                walls[x, y] = 2;
        //                i = new Vector2Int(x, y);
        //                queue.Add(i);
        //            }
        //        }
        //        queue.RemoveAt(0);
        //    }

        //    List<Vector2Int> remainingTiles = new List<Vector2Int>();
        //    for (int j = 0; j < walls.GetLength(0); j++) {
        //        for (int k = 0; k < walls.GetLength(1); k++) {
        //            if (walls[j, k] == 0) {
        //                i = new Vector2Int(j, k);
        //                remainingTiles.Add(i);
        //            }
        //        }
        //    }
        //    return remainingTiles.ToArray();
        //}

        public static int X(int index, int width) {
            return index%width;
        }

        public static int Y(int index, int width) {
            return index/width;
        }

    }
}
