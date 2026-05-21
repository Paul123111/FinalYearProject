using Agones;
using kcp2k;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class PlanetHunterNetworkManager : NetworkManager
{
    // Overrides the base singleton so we don't
    // have to cast to this type everywhere.
    public static new PlanetHunterNetworkManager singleton => (PlanetHunterNetworkManager)NetworkManager.singleton;
    public AgonesBetaSdk agones;

    bool shouldSpawnAstronaut = false;

    [Header("Custom Spawning Options")]
    [SerializeField] private GameObject astronautPrefab;
    [SerializeField] private GameObject rocketPrefab;
    bool firstScene = true;
    bool bluePlanet = false;
    bool redPlanet = false;
    bool icePlanet = false;

    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Awake()
    {
        base.Awake();
    }

    #region Unity Callbacks

    public override void OnValidate()
    {
        base.OnValidate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void LateUpdate()
    {
        base.LateUpdate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    #endregion

    #region Start & Stop

    /// <summary>
    /// Set the frame rate for a headless server.
    /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
    /// </summary>
    public override void ConfigureHeadlessFrameRate()
    {
        base.ConfigureHeadlessFrameRate();
    }

    /// <summary>
    /// called when quitting the application by closing the window / pressing stop in the editor
    /// </summary>
    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
    }

    #endregion

    #region Scene Management

    private bool isInitialServerBoot = true;

    /// <summary>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
    /// </summary>
    public override void OnClientSceneChanged() {
        base.OnClientSceneChanged();
        if (!NetworkClient.ready) {
            NetworkClient.Ready();
        }
    }

    #endregion

    #region Server System Callbacks

    /// <summary>
    /// Called on the server when a new client connects.
    /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerConnect(NetworkConnectionToClient conn) {
        base.OnServerConnect(conn);
    }

    [Server]
    public void TravelToPlanet(string sceneName) {
        shouldSpawnAstronaut = true;
        ServerChangeScene(sceneName);
    }

    [Server]
    public void TravelToSpace(string sceneName) {
        shouldSpawnAstronaut = false;
        // tearing down scene...
        StartCoroutine(SafeSceneTransitionRoutine(sceneName));
    }

    private IEnumerator SafeSceneTransitionRoutine(string targetScene) {
        Debug.Log("[NetworkManager] Initiating clean procedural scene purge...");

        // 1. Manually find and wipe the tile networks to initiate early cleanup
        ProcGenNetworking[] procGens = FindObjectsByType<ProcGenNetworking>();
        foreach (var procGen in procGens) {
            if (procGen != null) {
                // Force the object to clear its data layers ahead of schedule
                procGen.gameObject.SetActive(false);
                Destroy(procGen.gameObject);
            }
        }

        Tilemap[] maps = FindObjectsByType<Tilemap>();
        foreach (var tilemap in maps) {
            TilemapCollider2D tileCollider = tilemap.GetComponent<TilemapCollider2D>();
            CompositeCollider2D compCollider = tilemap.GetComponent<CompositeCollider2D>();
            Rigidbody2D rb2D = tilemap.GetComponent<Rigidbody2D>();

            if (tileCollider != null) tileCollider.enabled = false;
            if (compCollider != null) compCollider.enabled = false;
            if (rb2D != null) rb2D.simulated = false;

            Debug.Log("[NetworkManager] Tilemap physics disabled safely ahead of scene teardown.");
        }

        yield return new WaitForEndOfFrame();

        Debug.Log("[NetworkManager] Unmanaged tiles cleared. Safely migrating scenes.");

        ServerChangeScene(targetScene);
    }

    /// <summary>
    /// Called on the server when a client is ready.
    /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerReady(NetworkConnectionToClient conn) {
        GameObject oldRocket = conn.identity != null ? conn.identity.gameObject : null;
        Transform startPos = GetStartPosition();
        Vector3 spawnPos = startPos != null ? startPos.position : Vector3.zero;
        Quaternion spawnRot = startPos != null ? startPos.rotation : Quaternion.identity;
        GameObject prefab = shouldSpawnAstronaut ? astronautPrefab : rocketPrefab;

        if (conn.authenticationData is AuthResponseMessage session) {
            GameObject newPlayer = Instantiate(prefab, spawnPos, spawnRot);

            if (oldRocket != null) {
                // replace rocket with astronaut
                NetworkServer.ReplacePlayerForConnection(conn, newPlayer, ReplacePlayerOptions.KeepActive);
                Destroy(oldRocket, 0.2f);
            } else {
                NetworkServer.AddPlayerForConnection(conn, newPlayer);
            }

            // get player number
            if (session.localPlayerNumber < 1) {
                session.localPlayerNumber = CalculatePlayerNumber();
                if (session.localPlayerNumber == -1) {
                    conn.Disconnect();
                }
            }

            // keep auth data
            AuthResponseMessage msg = session;
            conn.authenticationData = msg;

            PlayerColour[] playerColours = conn.identity.gameObject.GetComponentsInChildren<PlayerColour>();
            foreach (PlayerColour playerColour in playerColours) {
                Debug.Log(msg.localPlayerNumber);
                playerColour.playerNum = msg.localPlayerNumber;
            }
        } else {
            Debug.LogWarning($"[OnServerReady] Failed to spawn. AuthData matching failed or prefab is missing.");
        }
        NetworkServer.SetClientReady(conn);
        NetworkServer.SpawnObjects();
    }

    /// <summary>
    /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    //public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    //{
    //    //base.OnServerAddPlayer(conn);

    //    // spawn player in random radius near centre
    //    Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * 5f;
    //    Vector3 spawnPos = new Vector3(randomCircle.x, randomCircle.y, 0);

    //    // immediately assigning count on server to prevent confusion when multiple players join at once
    //    if (conn.authenticationData is AuthResponseMessage session) {
    //        session.localPlayerNumber = CalculatePlayerNumber();
    //        if (session.localPlayerNumber == -1) {
    //            conn.Disconnect();
    //        }

    //        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    //        NetworkServer.AddPlayerForConnection(conn, player);

    //        PlayerColour playerColour = conn.identity.gameObject.GetComponent<PlayerColour>();
    //        playerColour.playerNum = session.localPlayerNumber;

    //        // session is only local, need to assign it here
    //        conn.authenticationData = session;

    //    } else {
    //        conn.Disconnect();
    //    }
    //}

    /// <summary>
    /// Called on the server when a client disconnects.
    /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override async void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.authenticationData is AuthResponseMessage session) {
            Debug.Log($"Removing {session.id} from player list");
            await agones.DeleteListValue("players", session.id);
        }
        base.OnServerDisconnect(conn);

        var gameserver = await agones.GameServer();
        bool serverEmpty = await agones.GetListLength("players") <= 0;
        if (gameserver.Status.State == "Allocated" && serverEmpty) {
            Debug.Log("No players! Shutting down server...");
            ShutdownServer();
        }
    }

    private async void ShutdownServer() {
        await agones.Shutdown();
        await Task.Delay(1000);
        Application.Quit();
    }

    /// <summary>
    /// Called on server when transport raises an error.
    /// <para>NetworkConnection may be null.</para>
    /// </summary>
    /// <param name="conn">Connection of the client...may be null</param>
    /// <param name="transportError">TransportError enum</param>
    /// <param name="message">String message of the error.</param>
    public override void OnServerError(NetworkConnectionToClient conn, TransportError transportError, string message) { }

    /// <summary>
    /// Called on server when transport raises an exception.
    /// <para>NetworkConnection may be null.</para>
    /// </summary>
    /// <param name="conn">Connection of the client...may be null</param>
    /// <param name="exception">Exception thrown from the Transport.</param>
    public override void OnServerTransportException(NetworkConnectionToClient conn, Exception exception) { }

    #endregion

    #region Client System Callbacks

    /// <summary>
    /// Called on the client when connected to a server.
    /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
    /// </summary>
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        if (!NetworkClient.ready) {
            NetworkClient.Ready();
        }
        NetworkClient.AddPlayer();
    }

    /// <summary>
    /// Called on clients when disconnected from a server.
    /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
    /// </summary>
    public override void OnClientDisconnect() { }

    /// <summary>
    /// Called on clients when a servers tells the client it is no longer ready.
    /// <para>This is commonly used when switching scenes.</para>
    /// </summary>
    public override void OnClientNotReady() { }

    /// <summary>
    /// Called on client when transport raises an error.</summary>
    /// </summary>
    /// <param name="transportError">TransportError enum.</param>
    /// <param name="message">String message of the error.</param>
    public override void OnClientError(TransportError transportError, string message) { }

    /// <summary>
    /// Called on client when transport raises an exception.</summary>
    /// </summary>
    /// <param name="exception">Exception thrown from the Transport.</param>
    public override void OnClientTransportException(Exception exception) { }

    #endregion

    #region Start & Stop Callbacks

    // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
    // their functionality, users would need override all the versions. Instead these callbacks are invoked
    // from all versions, so users only need to implement this one case.

    /// <summary>
    /// This is invoked when a host is started.
    /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartHost() { }

    /// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer() {
        base.OnStartServer();
    }

    /// <summary>
    /// This is invoked when the client is started.
    /// </summary>
    public override void OnStartClient() {
        base.OnStartClient();
    }

    /// <summary>
    /// This is called when a host is stopped.
    /// </summary>
    public override void OnStopHost() { }

    /// <summary>
    /// This is called when a server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnStopServer() {
        base.OnStopServer();
    }

    /// <summary>
    /// This is called when a client is stopped.
    /// </summary>
    public override void OnStopClient() { }

    #endregion

    int CalculatePlayerNumber() {
        // get set of currently used player seats (1, 2, 3, 4)
        HashSet<int> claimedSeats = NetworkServer.connections.Values
            .Where(c => c.authenticationData is AuthResponseMessage session && session.localPlayerNumber > 0)
            .Select(c => ((AuthResponseMessage)c.authenticationData).localPlayerNumber).ToHashSet();
        for (int i = 1; i <= 4; i++) {
            if (!claimedSeats.Contains(i)) return i;
        }
        return -1;
    }
}
