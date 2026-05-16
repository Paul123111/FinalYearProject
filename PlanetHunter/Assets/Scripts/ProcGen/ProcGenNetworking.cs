using Mirror;
using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;
using static ProcGen.ProcGenLib;

public class ProcGenNetworking : NetworkBehaviour
{
    // seed on server is the same as all others
    [SyncVar(hook = nameof(OnSeedSynchronized))] int worldSeed;

    [Header("Tiles")]
    [SerializeField] Tilemap ground;
    [SerializeField] TileBase[] groundTiles;
    [SerializeField] bool isWall = false;

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

    void Awake() {
        if (groundTiles.Length > 0 && groundTiles[0] != null) {
            groundTilesCode = new TileBase[groundTiles.Length+1];
            groundTilesCode[0] = null;
            for (int i = 0; i < groundTiles.Length; i++) {
                groundTilesCode[i+1] = groundTiles[i];
            }
        } else {
            groundTilesCode = groundTiles;
        }
    }

    public override void OnStartServer() {
        base.OnStartServer();
        InitialiseWorld();
    }

    [Server]
    public void InitialiseWorld() {
        tileTypes = new int[worldWidth + 1, worldHeight + 1];
        generations = 0;
        retries = 0;
        fullMap = false;
        worldSeed = (int)NetworkTime.time;
    }

    void OnSeedSynchronized(int oldSeed, int newSeed) {
        ClearTiles();
        tileTypes = ChooseTiles(worldWidth, worldHeight, groundTilesCode.Length, noiseThreshold, worldSeed);
        GetTiles();
    }

    void PlaceTiles() {
        for (int w = 0; w < tileTypes.GetLength(0); w++) {
            for (int h = 0; h < tileTypes.GetLength(1); h++) {
                ground.SetTile(new Vector3Int(w - (worldWidth / 2), h - (worldHeight / 2), 0), groundTilesCode[tileTypes[w, h]]);
            }
        }
    }
    void ClearTiles() {
        for (int w = 0; w < tileTypes.GetLength(0); w++) {
            for (int h = 0; h < tileTypes.GetLength(1); h++) {
                ground.SetTile(new Vector3Int(w - (worldWidth / 2), h - (worldHeight / 2), 0), groundTilesCode[0]);
            }
        }
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
    }

    void FixEnclosedAreas() {
        int[,] copy = new int[tileTypes.GetLength(0), tileTypes.GetLength(1)];
        // 4 bytes in int, so num bytes is 4*length
        Buffer.BlockCopy(tileTypes, 0, copy, 0, tileTypes.GetLength(0) * tileTypes.GetLength(1) * 4);
        bool enclosed = EnclosedAreasPresent(copy);
        while (enclosed && retries++ < maxRetries) {
            worldSeed = (int)NetworkTime.time;
        }
        Debug.Log("Enclosed Areas Present: " + enclosed);
    }
}
