using Agones;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;
using Mirror;

public class AgonesStartup : MonoBehaviour
{
    public string playerName;
    public string playerId;
    public string accessToken;
    public bool authRequiredForClients = true;
    PlayerAuthMessage authMessage;

    private async void Awake() {
        Debug.Log($"{Application.cloudProjectId}");
        try {
            Debug.Log("Auth Start");
            string profileName = GetCommandLineArg("-profile") ?? "DefaultPlayer";
            var options = new InitializationOptions();
            options.SetProfile(profileName);
            await UnityServices.InitializeAsync(options);

            PlayerAccountService.Instance.SignedIn += SignedIn;
            if (PlayerAccountService.Instance.IsSignedIn) {
                SignedIn();
                return;
            }
            try {
                if (authRequiredForClients) {
                    await PlayerAccountService.Instance.StartSignInAsync();
                } else {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log("Signed in anonymously, id is " + AuthenticationService.Instance.PlayerId);
                }
            } catch (PlayerAccountsException ex) {
                Debug.LogException(ex);
            } catch (RequestFailedException ex) {
                Debug.LogException(ex);
            }
        } catch (Exception ex) {
            Debug.LogError($"Error initializing Unity services {ex.Message}");
        }
        string token = AuthenticationService.Instance.AccessToken;
        Debug.Log($"BEARER_TOKEN: {token}");

        //NetworkManager.singleton.StartClient();
    }

    private async void SignedIn() {
        try {
            if (authRequiredForClients) {
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("SignIn is successful.");
                // Fetch the human-friendly display name from Unity Player Accounts.
                playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
                Debug.Log($"Player name: {playerName}");
            }
        } catch (AuthenticationException ex) {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        } catch (RequestFailedException ex) {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    // helper function for testing
    private string GetCommandLineArg(string name) {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == name && args.Length > i + 1) {
                return args[i + 1];
            }
        }
        return null;
    }

}
