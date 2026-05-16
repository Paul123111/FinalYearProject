using UnityEngine;

namespace ProcGen {
    public static class ProcGenLib {

        // pick tiles based on noise
        public static int[,] ChooseTiles(int worldWidth, int worldHeight, int numTypes, float noiseThreshold, int seed) {
            float[,] noise = GenerateNoise(worldWidth, worldHeight, seed);
            int[,] tiles = new int[worldWidth, worldHeight];
            for (int w = 0; w < tiles.GetLength(0); w++) {
                for (int h = 0; h < tiles.GetLength(1); h++) {
                    if (noise[w, h] < noiseThreshold) {
                        tiles[w, h] = 0;
                    } else {
                        tiles[w, h] = Random.Range(10, 10*numTypes) / 10;
                    }
                }
            }
            return tiles;
        }

        public static float[,] GenerateNoise(int worldWidth, int worldHeight, int seed) {
            Random.InitState(seed);
            // 2d array of 2d vectors in corners
            Vector2[,] grid = new Vector2[worldWidth + 1, worldHeight + 1];

            // setting up vectors at corners
            for (int w = 0; w < grid.GetLength(0); w++) {
                for (int h = 0; h < grid.GetLength(1); h++) {
                    grid[w, h] = new Vector2(w + Random.Range(-1f, 1f), h + Random.Range(-1f, 1f));
                }
            }

            // get coordinates of candidate points in grid 
            Vector2[,] points = new Vector2[worldWidth, worldHeight];
            for (int w = 0; w < points.GetLength(0); w++) {
                for (int h = 0; h < points.GetLength(1); h++) {
                    points[w, h] = new Vector2(w + 0.5f, h + 0.5f);
                }
            }

            // get dot product of distance and gradient vectors
            float[,,] dotProd = new float[worldWidth, worldHeight, 4];
            for (int w = 0; w < points.GetLength(0); w++) {
                for (int h = 0; h < points.GetLength(1); h++) {
                    dotProd[w, h, 0] = Vector2.Dot(points[w, h] - grid[w, h], grid[w, h]);
                    dotProd[w, h, 1] = Vector2.Dot(points[w, h] - grid[w + 1, h], grid[w + 1, h]);
                    dotProd[w, h, 2] = Vector2.Dot(points[w, h] - grid[w, h + 1], grid[w, h + 1]);
                    dotProd[w, h, 3] = Vector2.Dot(points[w, h] - grid[w + 1, h + 1], grid[w + 1, h + 1]);
                }
            }

            // linearly interpolate dot products
            float[,] lerp1 = new float[worldWidth, worldHeight];
            float[,] lerp2 = new float[worldWidth, worldHeight];
            float[,] lerp3 = new float[worldWidth, worldHeight];
            for (int w = 0; w < points.GetLength(0); w++) {
                for (int h = 0; h < points.GetLength(1); h++) {
                    lerp1[w, h] = Mathf.Lerp(dotProd[w, h, 0], dotProd[w, h, 1], 0.5f);
                    lerp2[w, h] = Mathf.Lerp(dotProd[w, h, 2], dotProd[w, h, 3], 0.5f);
                    lerp3[w, h] = Mathf.Lerp(lerp1[w, h], lerp2[w, h], 0.5f);
                }
            }
            return lerp3;

        }

        // cellular automata currently using Conway's game of life as a test
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

        [SerializeField] static int minNeighbours = 3;
        [SerializeField] static int minTypeNeighbours = 4;
        [SerializeField] static int maxNeighbours = 8;
        [SerializeField] static int createNeighbours = 1;

        public static int[,] MyGameStep(int[,] tiles, int gen, int maxGens) {
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
                Debug.Log("Stopped infinite");
                return tiles;
            }

            // step based on rules below
            bool veryEmpty = PercentEmpty(tiles) > 0.9f;
            for (int w = 0; w < tiles.GetLength(0); w++) {
                for (int h = 0; h < tiles.GetLength(1); h++) {
                    int type = tiles[w, h];
                    int typeNeighbours = GetNumNeighbours(tiles, w, h, type);
                    int empty = GetNumNeighbours(tiles, w, h, 0);
                    int grass = GetNumNeighbours(tiles, w, h, 1);
                    int water = GetNumNeighbours(tiles, w, h, 2);
                    int red = GetNumNeighbours(tiles, w, h, 3);
                    int neighbours = grass + water + red;
                    if (neighbours < minNeighbours) { tiles[w, h] = 0; } else if (neighbours > maxNeighbours) { tiles[w, h] = 0; } else if ((neighbours > createNeighbours && type == 0) || (neighbours > 1 && veryEmpty && type == 0)) { tiles[w, h] = grass >= water ? (grass > red ? 1 : 3) : (water >= red ? 2 : 3); } else if (typeNeighbours < minTypeNeighbours) { tiles[w, h] = 0; }
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
                Debug.Log("Stopped infinite");
                return tiles;
            }

            // step based on rules below
            bool veryEmpty = PercentEmpty(tiles) > 0.95f;
            for (int w = 0; w < tiles.GetLength(0); w++) {
                for (int h = 0; h < tiles.GetLength(1); h++) {
                    // get type of current tile
                    int type = tiles[w, h];
                    // neighbour values
                    int typeNeighbours = GetNumNeighbours(tiles, w, h, type);
                    int neighbours = 0;
                    int popularType = -1;

                    // important: first tile in array is the empty tile
                    int[] individualNeighbours = new int[numTypes-1];
                    for (int i = 0; i < numTypes-1; i++) {
                        individualNeighbours[i] = GetNumNeighbours(tiles, w, h, i+1);
                        neighbours += individualNeighbours[i];
                        // get most popular non-empty neighbour type
                        if (individualNeighbours[i] > popularType) {
                            popularType = i+1;
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
            Debug.Log("Generation Complete");
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
    }
}
