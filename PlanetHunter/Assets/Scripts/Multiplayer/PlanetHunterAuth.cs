using Agones;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Mirror;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

/*
    Documentation: https://mirror-networking.gitbook.io/docs/components/network-authenticators
    API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

public struct AuthRequestMessage : NetworkMessage {
    public string id;
    public string token;
}

public struct AuthResponseMessage : NetworkMessage {
    public bool success;
    public string errorMessage;
    public string id;
    public int localPlayerNumber; // 1-4
}

public class PlanetHunterAuth : NetworkAuthenticator
{
    #region Messages

    UnityTokenValidator tokenValidator = new UnityTokenValidator();

    public AgonesBetaSdk agones;

    #endregion

    #region Server

    /// <summary>
    /// Called on server from StartServer to initialize the Authenticator
    /// <para>Server message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartServer()
    {
        // register a handler for the authentication request from client
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    /// <summary>
    /// Called on server from OnServerConnectInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    public override void OnServerAuthenticate(NetworkConnectionToClient conn) { }

    /// <summary>
    /// Called on server when the client's AuthRequestMessage arrives
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    /// <param name="msg">The message payload</param>
    public async void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
    {
        try {
            if (conn == null || string.IsNullOrEmpty(msg.id)) return;

            bool tokenValid = await tokenValidator.ValidateToken(msg);
            // have to check connection is alive at each async step
            if (!NetworkAuthUtils.ConnectionAlive(conn)) return;
            if (!tokenValid) {
                await RejectClient(conn, "Could not validate token.");
                return;
            }

            bool duplicatePlayer = await agones.ListContains("players", msg.id);
            if (!NetworkAuthUtils.ConnectionAlive(conn)) return;
            if (duplicatePlayer) {
                await RejectClient(conn, $"{msg.id} is already connected!");
                return;
            }

            bool addPlayer = await agones.AppendListValue("players", msg.id);
            if (!NetworkAuthUtils.ConnectionAlive(conn)) return;
            if (addPlayer) {
                long count = await agones.GetListLength("players");
                Debug.Log(count);
            } else {
                await agones.DeleteListValue("players", msg.id);
                ServerReject(conn);
                await RejectClient(conn, "Could not join - Server Full!");
            }

        } catch (Exception ex) {
            Debug.LogException(ex);
        }

        if (!NetworkAuthUtils.ConnectionAlive(conn)) return;
        // send response message
        AuthResponseMessage authResponseMessage = new AuthResponseMessage { 
            success = true,
            errorMessage = "",
            id = msg.id,
            localPlayerNumber = -1
        };
        conn.authenticationData = authResponseMessage;
        conn.Send(authResponseMessage);

        // accept the successful authentication
        ServerAccept(conn);
    }

    /// <summary>
    /// Called when server stops, used to unregister message handlers if needed.
    /// </summary>
    public override void OnStopServer()
    {
        // Unregister the handler for the authentication request
        NetworkServer.UnregisterHandler<AuthRequestMessage>();
    }

    #endregion

    #region Client

    /// <summary>
    /// Called on client from StartClient to initialize the Authenticator
    /// <para>Client message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartClient()
    {
        // register a handler for the authentication response we expect from server
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    /// <summary>
    /// Called on client from OnClientConnectInternal when a client needs to authenticate
    /// </summary>
    public override void OnClientAuthenticate()
    {
        var msg = new AuthRequestMessage {
            id = AuthenticationService.Instance.PlayerId,
            token = AuthenticationService.Instance.AccessToken
        };
        Debug.Log(msg.id + ", token: " + msg.token);
        NetworkClient.Send(msg);
    }

    /// <summary>
    /// Called on client when the server's AuthResponseMessage arrives
    /// </summary>
    /// <param name="msg">The message payload</param>
    public void OnAuthResponseMessage(AuthResponseMessage msg)
    {
        if (msg.success) {
            Debug.Log("[Client] Authenticated successfully! Entering match...");
            ClientAccept();
        } else {
            Debug.Log($"[Client] Could not authenicate: {msg.errorMessage}");
            NetworkClient.Disconnect();
        }
    }

    /// <summary>
    /// Called when client stops, used to unregister message handlers if needed.
    /// </summary>
    public override void OnStopClient()
    {
        // Unregister the handler for the authentication response
        NetworkClient.UnregisterHandler<AuthResponseMessage>();
    }

    public async Task RejectClient(NetworkConnectionToClient conn, string errorMessage) {
        try {
            if (!NetworkAuthUtils.ConnectionAlive(conn)) return;
            AuthResponseMessage failureResponse = new AuthResponseMessage {
                success = false,
                errorMessage = errorMessage,
                id = "",
                localPlayerNumber = -1
            };
            conn.Send(failureResponse);
            // wait a bit to send failureResponse before rejecting client
            await Task.Delay(100);
            if (!NetworkAuthUtils.ConnectionAlive(conn)) return;
            ServerReject(conn);
        } catch (Exception ex) {
            Debug.LogException(ex);
        }
    }

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

    public async Task<bool> ValidateToken(AuthRequestMessage msg) {
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

public static class NetworkAuthUtils {
    public static bool ConnectionAlive(NetworkConnectionToClient conn) {
        return !(conn == null || !NetworkServer.connections.ContainsKey(conn.connectionId));
    }
}
