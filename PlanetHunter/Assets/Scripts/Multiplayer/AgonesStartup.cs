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
    AgonesAlphaSdk agones;
    bool ok = false;
    public string playerName;
    public string playerId;
    public string accessToken;


    private async void Awake() {
#if !UNITY_SERVER
        try {
            Debug.Log("Auth Start");
            await UnityServices.InitializeAsync();

            PlayerAccountService.Instance.SignedIn += SignedIn;
            if (PlayerAccountService.Instance.IsSignedIn) {
                SignedIn();
                return;
            }
            try {
                await PlayerAccountService.Instance.StartSignInAsync();
            } catch (PlayerAccountsException ex) {
                Debug.LogException(ex);
            } catch (RequestFailedException ex) {
                Debug.LogException(ex);
            }
        } catch (Exception ex) {
            Debug.LogError($"Error initializing Unity services {ex.Message}");
        }
#else
        Debug.Log("Skipping auth login - Server build detected");
#endif
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        agones = GetComponent<AgonesAlphaSdk>();

    }

    private async void SignedIn() {
        try {
            await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
            Debug.Log("SignIn is successful.");
            // Fetch the human-friendly display name from Unity Player Accounts.
            playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
            Debug.Log($"Player name: {playerName}");
        } catch (AuthenticationException ex) {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        } catch (RequestFailedException ex) {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }

        playerId = AuthenticationService.Instance.PlayerId;
        accessToken = AuthenticationService.Instance.AccessToken;
        playerName = AuthenticationService.Instance.PlayerName;

        PlayerAuthMessage message = new PlayerAuthMessage { id = playerId };
        //NetworkClient.Send(message);
        //ok = await agones.Connect();

        //if (ok) {
        //    var gameServer = await agones.GameServer();
        //    Debug.Log(gameServer.Status);
        //}

        //agones = FindFirstObjectByType<AgonesSdk2>(FindObjectsInactive.Include);

        //ok = await agones.Connect();

        //if (ok) {
        //    var gameServer = await agones.GameServer();
        //    Debug.Log(gameServer.Status);
        //}
    }

}
