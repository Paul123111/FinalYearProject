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
    /// This causes the server to switch scenes and sets the networkSceneName.
    /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
    /// </summary>
    /// <param name="newSceneName"></param>
    //public override void ServerChangeScene(string newSceneName)
    //{
    //    if (string.IsNullOrEmpty(newSceneName)) return;
    //    if (isInitialServerBoot) {
    //        isInitialServerBoot = false;
    //        base.ServerChangeScene(newSceneName);
    //        return;
    //    }

    //    networkSceneName = newSceneName;
    //    StartCoroutine(ServerLoadSceneAsync(newSceneName));
    //}

    //private IEnumerator ServerLoadSceneAsync(string newSceneName) {
    //    OnServerChangeScene(newSceneName);
    //    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Single);
    //    if (asyncLoad == null) {
    //        Debug.LogError($"[FATAL] Scene '{newSceneName}' could not be loaded async! Is it missing from Build Settings?");
    //        yield break;
    //    }
    //    while (!asyncLoad.isDone) {
    //        yield return null;
    //    }
    //    yield return new WaitForSeconds(1f);
    //    Scene loadedScene = SceneManager.GetSceneByName(newSceneName);
    //    if (loadedScene.IsValid() && loadedScene.isLoaded) {
    //        try {
    //            SceneManager.SetActiveScene(loadedScene);
    //        } catch (Exception ex) {
    //            Debug.LogError($"[SERVER] SetActiveScene crashed despite scene being valid: {ex.Message}");
    //        }
    //    }
    //    NetworkServer.SetAllClientsNotReady();
    //    base.OnServerSceneChanged(newSceneName);
    //    foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values) {
    //        if (conn?.identity != null) {
    //            NetworkServer.RebuildObservers(conn.identity, true);
    //        }
    //    }
    //    NetworkServer.SpawnObjects();
    //}

    /// <summary>
    /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    public override void OnServerChangeScene(string newSceneName) { }

    /// <summary>
    /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
    /// </summary>
    /// <param name="sceneName">The name of the new scene.</param>
    //public override void OnServerSceneChanged(string sceneName) {
    //    base.OnServerSceneChanged(sceneName);
    //}

    /// <summary>
    /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    /// <param name="sceneOperation">Scene operation that's about to happen</param>
    /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
    //public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) {
    //    if (NetworkServer.active && !NetworkClient.isConnected) return;
    //    NetworkClient.isLoadingScene = true;

    //    StartCoroutine(ClientLoadSceneAsync(newSceneName));
    //}

    //private IEnumerator ClientLoadSceneAsync(string newSceneName) {
    //    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Single);
    //    if (asyncLoad == null) {
    //        Debug.LogError($"[FATAL] Scene '{newSceneName}' could not be loaded async! Is it missing from Build Settings?");
    //        yield break;
    //    }
    //    while (!asyncLoad.isDone) {
    //        yield return null;
    //    }
    //    yield return new WaitForSeconds(1f);
    //    Scene loadedScene = SceneManager.GetSceneByName(newSceneName);
    //    if (loadedScene.IsValid()) {
    //        SceneManager.SetActiveScene(loadedScene);
    //        Debug.Log($"[SERVER] Active scene context successfully forced to: '{newSceneName}'");
    //    } else {
    //        Debug.LogError($"[SERVER] Failed to grab valid scene context for '{newSceneName}'!");
    //    }
    //    OnClientSceneChanged();
    //}

    /// <summary>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
    /// </summary>
    public override void OnClientSceneChanged() {
        base.OnClientSceneChanged();
        if (!NetworkClient.ready) {
            NetworkClient.Ready();
        }
        //if (NetworkClient.localPlayer == null) {
        //    NetworkClient.AddPlayer();
        //}
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
        ServerChangeScene(sceneName);
    }

    /// <summary>
    /// Called on the server when a client is ready.
    /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        if (shouldSpawnAstronaut) {
            GameObject oldRocket = conn.identity != null ? conn.identity.gameObject : null;
            firstScene = true;

            Transform startPos = GetStartPosition();
            Vector3 spawnPos = startPos != null ? startPos.position : Vector3.zero;
            Quaternion spawnRot = startPos != null ? startPos.rotation : Quaternion.identity;

            if (conn.authenticationData is AuthResponseMessage session) {
                // keep auth data
                AuthResponseMessage msg = session;
                // replace rocket with astronaut
                GameObject newAstronaut = Instantiate(astronautPrefab, spawnPos, spawnRot);
                NetworkServer.ReplacePlayerForConnection(conn, newAstronaut, ReplacePlayerOptions.KeepActive);
                conn.authenticationData = msg;
                PlayerColour[] playerColours = conn.identity.gameObject.GetComponentsInChildren<PlayerColour>();
                foreach (PlayerColour playerColour in playerColours) {
                    playerColour.playerNum = msg.localPlayerNumber;
                }
            }

            if (oldRocket != null) {
                Destroy(oldRocket, 0.2f);
            }
            base.OnServerReady(conn);
        } else if (firstScene) {
            base.OnServerReady(conn);
        } else {
            firstScene = true;
            GameObject oldRocket = conn.identity != null ? conn.identity.gameObject : null;

            Transform startPos = GetStartPosition();
            Vector3 spawnPos = startPos != null ? startPos.position : Vector3.zero;
            Quaternion spawnRot = startPos != null ? startPos.rotation : Quaternion.identity;

            if (conn.authenticationData is AuthResponseMessage session) {
                // keep auth data
                AuthResponseMessage msg = session;
                // replace rocket with astronaut
                GameObject newAstronaut = Instantiate(rocketPrefab, spawnPos, spawnRot);
                NetworkServer.ReplacePlayerForConnection(conn, newAstronaut, ReplacePlayerOptions.KeepActive);
                conn.authenticationData = msg;
                PlayerColour[] playerColours = conn.identity.gameObject.GetComponentsInChildren<PlayerColour>();
                foreach (PlayerColour playerColour in playerColours) {
                    playerColour.playerNum = msg.localPlayerNumber;
                }
            }

            if (oldRocket != null) {
                Destroy(oldRocket, 0.2f);
            }
            base.OnServerReady(conn);
        }
        NetworkServer.SpawnObjects();
    }

    /// <summary>
    /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        //base.OnServerAddPlayer(conn);

        // spawn player in random radius near centre
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * 5f;
        Vector3 spawnPos = new Vector3(randomCircle.x, randomCircle.y, 0);

        // immediately assigning count on server to prevent confusion when multiple players join at once
        if (conn.authenticationData is AuthResponseMessage session) {
            session.localPlayerNumber = CalculatePlayerNumber();
            if (session.localPlayerNumber == -1) {
                conn.Disconnect();
            }

            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, player);

            PlayerColour playerColour = conn.identity.gameObject.GetComponent<PlayerColour>();
            playerColour.playerNum = session.localPlayerNumber;

            // session is only local, need to assign it here
            conn.authenticationData = session;

        } else {
            conn.Disconnect();
        }
    }

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
    public override void OnStopServer() { }

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
