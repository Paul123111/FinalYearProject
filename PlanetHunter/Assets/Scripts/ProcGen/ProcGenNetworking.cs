using Mirror;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using static ProcGen.ProcGenLib;

public class ProcGenNetworking : NetworkBehaviour
{
    // seed on server is the same as all others
    [SyncVar] int worldSeed = 0;

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
    int[] tileTypes; // 2d array for tile types
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

    [Header("Batching for Online")]
    [SerializeField] int batchSize = 10;

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
        tilesToMoveHash = new HashSet<int>(tilesToMove);
    }

    public override void OnStartServer() {
        base.OnStartServer();
        InitialiseWorld();
    }

    public override void OnStartClient() {
        base.OnStartClient();
        Debug.Log(worldSeed);
        GenerateWorld();
    }

    [Server]
    public void InitialiseWorld() {
        generations = 0;
        retries = 0;
        fullMap = false;
        worldSeed = (int) Environment.TickCount;
        Debug.Log($"Seed: {worldSeed}, initialising world...");
        GenerateWorld();
    }

    async Task GenerateWorld() {
        await Task.Delay(2000);
        tileTypes = ChooseTiles(worldWidth, worldHeight, groundTilesCode.Length, noiseThreshold, worldSeed);
        //ClearTiles();
        GetTiles();
    }

    void PlaceTiles() {
        TileBase[] placeArray = new TileBase[tileTypes.Length];
        TileBase[] separate = null;
        //int[] separateTypes = new int[tileTypes.Length];
        //if (separateLayer != null) {
        //    separateTypes = SeparateTiles(tileTypes);
        //}
        for (int i = 0; i < tileTypes.Length; i++) {
            placeArray[i] = groundTilesCode[tileTypes[i]];
            //if (separateLayer != null) {
            //    separate[i] = groundTilesCode[separateTypes[i]];
            //}
        }
        BlockTilePlace(-worldWidth/2, -worldHeight/2, worldWidth/2, worldHeight/2, placeArray, separate);
        //LoopTiles();
    }

    //void PlaceTile(int w, int h) {
    //    if (tileTypes[] != 0) {
    //        // put relevant tiles on separateLayer
    //        if (separateLayer != null && tilesToMoveHash.Contains(tileTypes[w, h])) {
    //            separateLayer.SetTile(new Vector3Int(w - (worldWidth / 2), h - (worldHeight / 2), 0), groundTilesCode[tileTypes[w, h]]);
    //        } else {
    //            ground.SetTile(new Vector3Int(w - (worldWidth / 2), h - (worldHeight / 2), 0), groundTilesCode[tileTypes[w, h]]);
    //        }
    //    } else {
    //        ground.SetTile(new Vector3Int(w - (worldWidth / 2), h - (worldHeight / 2), 0), defaultTile);
    //    }
    //}

    void ClearTile(int w, int h) {
        ground.SetTile(new Vector3Int(w - (worldWidth / 2), h - (worldHeight / 2), 0), groundTilesCode[0]);
        if (separateLayer != null) {
            separateLayer.SetTile(new Vector3Int(w - (worldWidth / 2), h - (worldHeight / 2), 0), groundTilesCode[0]);
        }
    }

    //void ClearTiles() {
    //    for (int w = 0; w < tileTypes.GetLength(0); w++) {
    //        for (int h = 0; h < tileTypes.GetLength(1); h++) {
    //            ClearTile(w, h);
    //        }
    //    }
    //}

    void GetTiles() {
        //ClearTiles();
        for (int i = 0; i < maxGens + 1; i++) {
            tileTypes = PlanetStep(tileTypes, worldWidth, groundTilesCode.Length, generations++, maxGens,
                minNeighbours, minTypeNeighbours, maxNeighbours, createNeighbours);
            fullMap = FullMap(tileTypes);
        }
        //if (isWall) {
        //    FixEnclosedAreas();
        //}
        PlaceTiles();
    }

    void BlockTilePlace(int startX, int startY, int endX, int endY, TileBase[] tilemap, TileBase[] separate) {
        BoundsInt area = new BoundsInt(startX, startY, 0, endX-startX, endY-startY, 1);
        Debug.Log(area.size + ", " + tilemap.Length);
        ground.SetTilesBlock(area, tilemap);
        if (separateLayer != null && separate != null) {
            separateLayer.SetTilesBlock(area, separate);
        }
    }

    int[] SeparateTiles(int[] tiles) {
        int l = tiles.Length;
        int[] separateTiles = new int[l];
        for (int i = 0; i < l; i++) {
            if (tilesToMoveHash.Contains(tiles[i])) {
                separateTiles[i] = tiles[i];
                tiles[i] = 0;
            } else {
                separateTiles[i] = 0;
            }
        }
        return separateTiles;
    }

    //void FixEnclosedAreas() {
    //    int[,] copy = new int[tileTypes.GetLength(0), tileTypes.GetLength(1)];
    //    // 4 bytes in int, so num bytes is 4*length
    //    Buffer.BlockCopy(tileTypes, 0, copy, 0, tileTypes.GetLength(0) * tileTypes.GetLength(1) * 4);
    //    bool enclosed = EnclosedAreasPresent(copy);
    //    while ((enclosed || fullMap) && retries++ < maxRetries) {
    //        worldSeed = (int)NetworkTime.time;
    //    }
    //    if (retries >= maxRetries) {
    //        Debug.Log("Enclosed Areas Present: " + enclosed);
    //    }
    //}

    // for the illusion of looping (copyX starts at top left corner, e.g. if copyX == startX, the duplicate would overlap)
    //void DuplicateTiles(int startX, int startY, int endX, int endY, int copyX, int copyY) {
    //    if (startX > endX || startY > endY) {
    //        Debug.LogError("StartXY should be lower than endXY");
    //        return;
    //    }
    //    // get tile clipboard
    //    int[,] copy = new int[endX - startX, endY - startY];
    //    for (int x = startX; x < endX; x++) {
    //        for (int y = startY; y < endY; y++) {
    //            copy[x - startX, y - startY] = tileTypes[x, y];
    //        }
    //    }

    //    // paste tiles into tilemap
    //    for (int x = copyX; x < copyX + copy.GetLength(0); x++) {
    //        for (int y = copyY; y < copyY + copy.GetLength(1); y++) {
    //            PlaceTileCustom(x, y, copyX, copyY, copy);
    //        }
    //    }
    //}

    //void PlaceTileCustom(int w, int h, int offsetX, int offsetY, int[,] tiles) {
    //    int currentTile = tiles[w - offsetX, h - offsetY];
    //    if (currentTile != 0) {
    //        // put relevant tiles on separateLayer
    //        if (separateLayer != null && tilesToMoveHash.Contains(currentTile)) {
    //            separateLayer.SetTile(new Vector3Int(w - (worldWidth / 2), h - (worldHeight / 2), 0), groundTilesCode[currentTile]);
    //        } else {
    //            ground.SetTile(new Vector3Int(w - (worldWidth / 2), h - (worldHeight / 2), 0), groundTilesCode[currentTile]);
    //        }
    //    } else {
    //        ground.SetTile(new Vector3Int(w - (worldWidth / 2), h - (worldHeight / 2), 0), defaultTile);
    //    }
    //}

    //void LoopTiles() {
    //    DuplicateTiles(0, 0, worldWidth, worldHeight, -worldWidth, worldHeight); // top-left
    //    DuplicateTiles(0, 0, worldWidth, worldHeight, 0, worldHeight); // top-centre
    //    DuplicateTiles(0, 0, worldWidth, worldHeight, worldWidth, worldHeight); // top-right
    //    DuplicateTiles(0, 0, worldWidth, worldHeight, -worldWidth, 0); // centre-left
    //    DuplicateTiles(0, 0, worldWidth, worldHeight, worldWidth, 0); // centre-right
    //    DuplicateTiles(0, 0, worldWidth, worldHeight, -worldWidth, -worldHeight); // top-left
    //    DuplicateTiles(0, 0, worldWidth, worldHeight, 0, -worldHeight); // top-centre
    //    DuplicateTiles(0, 0, worldWidth, worldHeight, worldWidth, -worldHeight); // top-right
    //}

    //IEnumerator ClearTilesTimeSliced() {
    //    int tilesProcessedThisFrame = 0;
    //    for (int x = 0; x < worldWidth; x++) {
    //        for (int y = 0; y < worldHeight; y++) {
    //            ClearTile(x, y);
    //            tilesProcessedThisFrame++;

    //            if (tilesProcessedThisFrame >= batchSize) {
    //                tilesProcessedThisFrame = 0;
    //                yield return null;
    //            }
    //        }
    //    }
    //    Debug.Log("[WorldGen] World generation completed cleanly without thread lockups.");
    //}

    //IEnumerator PlaceTilesTimeSliced() {
    //    int tilesProcessedThisFrame = 0;
    //    for (int x = 0; x < worldWidth; x++) {
    //        for (int y = 0; y < worldHeight; y++) {
    //            PlaceTile(x, y);
    //            tilesProcessedThisFrame++;

    //            if (tilesProcessedThisFrame >= batchSize) {
    //                tilesProcessedThisFrame = 0;
    //                yield return null;
    //            }
    //        }
    //    }
    //    Debug.Log("[WorldGen] World generation completed cleanly without thread lockups.");
    //}

    //IEnumerator GetTilesTimeSliced() {
    //    int tilesProcessedThisFrame = 0;
    //    for (int i = 0; i < maxGens + 1; i++) {
    //        tileTypes = PlanetStep(tileTypes, groundTilesCode.Length, generations++, maxGens,
    //            minNeighbours, minTypeNeighbours, maxNeighbours, createNeighbours);
    //        fullMap = FullMap(tileTypes);

    //        tilesProcessedThisFrame++;
    //        if (tilesProcessedThisFrame >= batchSize) {
    //            tilesProcessedThisFrame = 0;
    //            yield return null;
    //        }
    //    }
    //    yield return StartCoroutine(PlaceTilesTimeSliced());
    //}

}
