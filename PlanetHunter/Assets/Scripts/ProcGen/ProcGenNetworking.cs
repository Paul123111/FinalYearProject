using Items;
using Mirror;
using ProcGen;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static ProcGen.ProcGenLib;

public class ProcGenNetworking : NetworkBehaviour {
    // seed on server is the same as all others
    [SyncVar(hook = nameof(GenerateSeed))] int worldSeed = 0;

    [Header("Tiles")]
    [SerializeField] Tilemap ground;
    [SerializeField] TileBase[] groundTiles;
    [SerializeField] TileBase defaultTile;
    [SerializeField] bool isWall = false;
    // set this if you would like some tiles to be moved to different tilemap, e.g. water from walls
    // must be same size as current tilemap
    [SerializeField] Tilemap separateLayer;
    [SerializeField] int[] tilesToMove; // by 1-based index in groundTiles (because empty is 0)
    HashSet<int> tilesToMoveHash;

    // extra array for automatically adding empty tile to index 0
    TileBase[] groundTilesCode;
    int[,] tileTypes; // 2d array for tile types
    bool fullMap = false;

    [Header("Generations")]
    [SerializeField] int maxGens = 1000;
    int generations = 0;
    [SerializeField] int maxRetries = 20; // max number of times to retry if enclosed
    int retries = 0;

    [Header("World Size")]
    [SerializeField] int worldWidth = 100;
    [SerializeField] int worldHeight = 100;

    [Header("Neighbour Rules")]
    [SerializeField] int minNeighbours = 3;
    [SerializeField] int minTypeNeighbours = 4;
    [SerializeField] int maxNeighbours = 8;
    [SerializeField] int createNeighbours = 1;
    [SerializeField] float noiseThreshold = 15f;

    [Header("Enemy Rules")]
    [SerializeField] GameObject enemy;
    [SerializeField] GameObject pickup;
    [SerializeField] EquipmentDatabase equipmentDatabase;
    Vector2Int[] spawns;

    void InitialiseTileArrays() {
        if (groundTiles.Length > 0 && groundTiles[0] != null) {
            groundTilesCode = new TileBase[groundTiles.Length + 1];
            groundTilesCode[0] = null;
            for (int i = 0; i < groundTiles.Length; i++) {
                groundTilesCode[i + 1] = groundTiles[i];
            }
        } else {
            groundTilesCode = groundTiles;
        }
        tilesToMoveHash = new HashSet<int>(tilesToMove);
        spawns = new Vector2Int[10];
    }

    void Awake() {
        InitialiseTileArrays();
    }

    public override void OnStartServer() {
        base.OnStartServer();
        if (groundTilesCode == null) {
            InitialiseTileArrays();
        }
        InitialiseWorld();
    }

    [Server]
    public void InitialiseWorld() {
        worldSeed = (int)Environment.TickCount;
        GenerateSeed(0, worldSeed);
        Debug.Log($"Seed: {worldSeed}, initialising world...");
    }

    void GenerateSeed(int old, int newV) {
        generations = 0;
        fullMap = false;
        CreateWorld(newV);
    }

    void CreateWorld(int seed) {
        tileTypes = ChooseTiles(worldWidth, worldHeight, groundTilesCode.Length, noiseThreshold, seed);
        GetTiles();
    }


    //void GenerateWorld() {
    //    Debug.Log("Synchronising seed...");
    //    tileTypes = ChooseTiles(worldWidth, worldHeight, groundTilesCode.Length, noiseThreshold, worldSeed);
    //    GetTiles();
    //}

    void PlaceTiles() {
        TileBase[] tiles = new TileBase[tileTypes.GetLength(0) * tileTypes.GetLength(1)];
        TileBase[] separate = new TileBase[tileTypes.GetLength(0) * tileTypes.GetLength(1)];
        for (int x = 0; x < tileTypes.GetLength(0); x++) {
            for (int y = 0; y < tileTypes.GetLength(1); y++) {
                var t = tileTypes[x, y];
                if (tilesToMoveHash.Contains(t)) {
                    separate[x + (y * worldHeight)] = groundTilesCode[t];
                } else {
                    tiles[x + (y * worldHeight)] = groundTilesCode[t];
                }
            }
        }
        int w = worldWidth / 2;
        int h = worldHeight / 2;
        BlockTilePlace(-w*3, -h, -w, h, tiles, separate); // left
        BlockTilePlace(-w, -h, w, h, tiles, separate); // center
        BlockTilePlace(w, -h, w*3, h, tiles, separate); // right

        // top
        BlockTilePlace(-w*3, h, -w, h*3, tiles, separate); // left
        BlockTilePlace(-w, h, w, h*3, tiles, separate); // center
        BlockTilePlace(w, h, w*3, h*3, tiles, separate); // right

        // bottom
        BlockTilePlace(-w * 3, -h*3, -w, -h, tiles, separate); // left
        BlockTilePlace(-w, -h*3, w, -h, tiles, separate); // center
        BlockTilePlace(w, -h*3, w * 3, -h, tiles, separate); // right
        //LoopTiles();
        //for (int w = 0; w < tileTypes.GetLength(0); w++) {
        //    for (int h = 0; h < tileTypes.GetLength(1); h++) {
        //        PlaceTile(w, h);
        //    }
        //}
    }

    void ClearTiles() {
        TileBase[] tiles = new TileBase[tileTypes.GetLength(0) * tileTypes.GetLength(1) * 9];
        for (int i = 0; i < tiles.Length; i++) {
            tiles[i] = null;
        }
        int w = worldWidth / 2;
        int h = worldHeight / 2;
        BlockTilePlace(-w*3, -h*3, w*3, h*3, tiles, null);
    }

    void GetTiles() {
        ClearTiles();
        for (int i = 0; i < maxGens + 1; i++) {
            tileTypes = PlanetStep(tileTypes, groundTilesCode.Length, generations++, maxGens,
                minNeighbours, minTypeNeighbours, maxNeighbours, createNeighbours);
            fullMap = FullMap(tileTypes);
        }
        if (isWall) {
            FixEnclosedAreas();
        }
        PlaceTiles();
        if (isWall) {
            SpawnEnemies(enemy, worldSeed);
            SpawnEnemies(pickup, worldSeed*2);
            SpawnEnemies(pickup, worldSeed*3);
        }
    }

    void FixEnclosedAreas() {
        int[,] copy = new int[tileTypes.GetLength(0), tileTypes.GetLength(1)];
        // 4 bytes in int, so num bytes is 4*length
        Array.Copy(tileTypes, copy, tileTypes.Length);
        int localSeed = worldSeed;
        bool enclosed = EnclosedAreasPresent(copy);
        while ((enclosed || fullMap) && retries < maxRetries) {
            PseudoRandomRange(0, 999999, localSeed, out localSeed);
            tileTypes = ChooseTiles(worldWidth, worldHeight, groundTilesCode.Length, noiseThreshold, localSeed);
            Array.Copy(tileTypes, copy, tileTypes.Length);
            enclosed = EnclosedAreasPresent(copy);
            retries++;
        }
        if (retries >= maxRetries) {
            Debug.Log("Enclosed Areas Present: " + enclosed);
        }
    }

    void BlockTilePlace(int startX, int startY, int endX, int endY, TileBase[] tilemap, TileBase[] separate) {
        BoundsInt area = new BoundsInt(startX, startY, 0, endX - startX, endY - startY, 1);
        //Debug.Log(area.size + ", " + tilemap.Length);
        ground.SetTilesBlock(area, tilemap);
        if (separateLayer != null && separate != null) {
            separateLayer.SetTilesBlock(area, separate);
        }
    }

    void SpawnEnemies(GameObject prefab, int seed) {
        if (!NetworkServer.active) return;
        int rand = seed;
        for (int i = 0; i < spawns.Length; i++) {
            int loopSafety = 0;
            spawns[i] = new Vector2Int(PseudoRandomRange(0, 99, rand, out rand), PseudoRandomRange(0, 99, rand, out rand));
            while (tileTypes[spawns[i].x, spawns[i].y] != 0 && loopSafety < 10) {
                loopSafety++;
                spawns[i] = new Vector2Int(PseudoRandomRange(0, 99, rand, out rand), PseudoRandomRange(0, 99, rand, out rand));
            }
            GameObject e = Instantiate(prefab, new Vector3(spawns[i].x-50, spawns[i].y-50, 0), Quaternion.identity);
            NetworkServer.Spawn(e);
        }
    }

    public Vector3Int SpawnPoint() {
        var point = GetRandomSpawn(worldSeed, ground, separateLayer);
        return new Vector3Int(point.x, point.y, 0);
    }
}