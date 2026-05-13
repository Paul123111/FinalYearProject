using Agones;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Mirror;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class PlanetHunterNetworkManager : NetworkManager
{
    // Overrides the base singleton so we don't
    // have to cast to this type everywhere.
    public static new PlanetHunterNetworkManager singleton => (PlanetHunterNetworkManager)NetworkManager.singleton;
    public AgonesAlphaSdk agones;

    Dictionary<NetworkConnectionToClient, string> _playerIds = new Dictionary<NetworkConnectionToClient, string>();
    long count = 0;
    NetworkConnectionToClient[] playerOrder = new NetworkConnectionToClient[4];

    UnityTokenValidator tokenValidator = new UnityTokenValidator();

    public async Task<long> GetPlayerCount() {
        return await agones.GetPlayerCount();
    }

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

    /// <summary>
    /// This causes the server to switch scenes and sets the networkSceneName.
    /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
    /// </summary>
    /// <param name="newSceneName"></param>
    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
    }

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
    public override void OnServerSceneChanged(string sceneName) { }

    /// <summary>
    /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    /// <param name="sceneOperation">Scene operation that's about to happen</param>
    /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) { }

    /// <summary>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
    /// </summary>
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
    }

    #endregion

    #region Server System Callbacks

    /// <summary>
    /// Called on the server when a new client connects.
    /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerConnect(NetworkConnectionToClient conn) {
    
    }

    /// <summary>
    /// Called on the server when a client is ready.
    /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
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
        // syncs with AgonesAlphaSdk.PlayerCount() later
        long assignedNum = -1;
        for (int i = 0; i < playerOrder.Length; i++) {
            if (playerOrder[i] == null) {
                playerOrder[i] = conn;
                assignedNum = i+1;
                break;
            }
        }

        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player);

        PlayerColour playerColour = conn.identity.gameObject.GetComponent<PlayerColour>();
        playerColour.playerNum = assignedNum;
    }

    /// <summary>
    /// Called on the server when a client disconnects.
    /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override async void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        string pid;
        _playerIds.TryGetValue(conn, out pid);
        _playerIds.Remove(conn);
        int index = Array.FindIndex(playerOrder, x => x == conn);
        if (index >= 0 && index < playerOrder.Length) {
            playerOrder[index] = null;
        }
        await agones.PlayerDisconnect(pid);
        count = await agones.GetPlayerCount();

        var gameserver = await agones.GameServer();
        if (gameserver.Status.State == "Allocated" && count <= 0) {
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
        var msg = new PlayerAuthMessage {
            id = AuthenticationService.Instance.PlayerId,
            token = AuthenticationService.Instance.AccessToken
        };
        Debug.Log(msg.id + ", token: " + msg.token);
        NetworkClient.Send(msg);
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

    public GameObject mirrorServer;
    /// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer() {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PlayerAuthMessage>(OnPlayerAuthReceived);
        Debug.Log("handle started");
    }

    private async void OnPlayerAuthReceived(NetworkConnectionToClient conn, PlayerAuthMessage msg) {
        try {
            Debug.Log($"SDK: {agones != null}, Id: {msg.id}");
            if (_playerIds.ContainsValue(msg.id)) {
                Debug.Log(msg.id + " is already connected!");
                conn.Disconnect();
                return;
            }
            Debug.Log("Validating token...");
            bool tokenValid = await tokenValidator.ValidateToken(msg);
            if (!tokenValid) {
                Debug.Log("Could not validate token. Kicking player...");
                conn.Disconnect();
                return;
            }
            Debug.Log("Validated token!");
            _playerIds.Add(conn, msg.id);
            bool ok = await agones.PlayerConnect(msg.id);
            if (ok) {
                count = await agones.GetPlayerCount();
            } else {
                Debug.Log("Server Full! Kicking player...");
                _playerIds.Remove(conn);
                conn.Disconnect();
            }
        } catch (Exception ex) {
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// This is invoked when the client is started.
    /// </summary>
    public override void OnStartClient() { }

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
    }

//unity has no OIDC discovery document, so get the JWKS and add to an openidConnectConfig for auth
public class UnityJwksRetriever : IConfigurationRetriever<OpenIdConnectConfiguration> {
    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel) {
        string doc = await retriever.GetDocumentAsync(address, cancel);
        var jwks = new JsonWebKeySet(doc);
        var config = new OpenIdConnectConfiguration();
        foreach (var key in jwks.GetSigningKeys()) {
            config.SigningKeys.Add(key);
        }
        return config;
    }
}

// validate jwt token
public class UnityTokenValidator {
    private ConfigurationManager<OpenIdConnectConfiguration> _configManager;

    public UnityTokenValidator() {
        var jwksUrl = "https://player-auth.services.api.unity.com/.well-known/jwks.json";
        _configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            jwksUrl,
            new UnityJwksRetriever());
    }

    public async Task<bool> ValidateToken(PlayerAuthMessage msg) {
        try {
            var config = await _configManager.GetConfigurationAsync();
            var handler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidIssuer = "https://player-auth.services.api.unity.com",

                ValidateAudience = true,
                AudienceValidator = (audiences, securityToken, validationParameters) => {
                    string projectId = "b3e96b7f-8284-47ea-bec0-90e51607c3b9";
                    return audiences.Any(aud => aud.Contains(projectId));
                },
                ValidateLifetime = true,
                ConfigurationManager = _configManager,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => {
                    var config = _configManager.GetConfigurationAsync().Result;
                    return config.SigningKeys;
                }
            };

            var principal = handler.ValidateToken(msg.token, validationParameters, out var validatedToken);
            return true;
        } catch (Exception ex) {
            Debug.LogError($"Could not authenticate player: {ex}");
            return false;
        }
    }
}
