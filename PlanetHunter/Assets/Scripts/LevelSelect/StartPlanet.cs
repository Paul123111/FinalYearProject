using Mirror;
using ProcGen;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartPlanet : NetworkBehaviour
{
    [SerializeField] Image countdownUI;
    TextMeshProUGUI countdownText;
    //[SyncVar(hook = nameof(OnCountdownStart))]
    bool startedCountdown = false;

    [SyncVar] double startTime;
    readonly SyncList<PlanetSelect> voteList = new SyncList<PlanetSelect>();
    [SerializeField] double countdownLength;

    bool changingScene = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        countdownText = countdownUI.GetComponentInChildren<TextMeshProUGUI>();

        startedCountdown = false;
        countdownUI.enabled = false;
        countdownText.enabled = false;
    }

    public override void OnStartServer() {
        voteList.OnAdd += OnVoteAdd;
        voteList.OnRemove += OnVoteRemove;
    }

    private void Update() {
        if (startedCountdown) {
            countdownText.text = $"Start in {Mathf.CeilToInt((float)(startTime + countdownLength-NetworkTime.time))}...";
            if (NetworkTime.time > startTime+countdownLength) {
                int index = ProcGenLib.PseudoRandomRange(0, voteList.Count, 123, out int rand);
                countdownText.text = $"Landing on {voteList[index].name}!";
                startedCountdown = false;
                if (!changingScene) {
                    changingScene = true;
                    if (voteList[index].name == "BluePlanet") {
                        PlanetHunterNetworkManager.singleton.TravelToPlanet("AstronautTest");
                    } else if (voteList[index].name == "IcePlanet") {
                        PlanetHunterNetworkManager.singleton.TravelToPlanet("ice");
                    } else if (voteList[index].name == "RedPlanet") {
                        PlanetHunterNetworkManager.singleton.TravelToPlanet("red");
                    }
                }
            }
        }
    }

    [Server]
    public void CountdownStart() {
        if (startedCountdown) return;
        startedCountdown = true;
        startTime = NetworkTime.time;
        CountdownUIClient(true);
    }

    [Server]
    public void CountdownStop() {
        startedCountdown = false;
        CountdownUIClient(false);
    }

    [Server]
    public void VoteForPlanet(PlanetSelect vote, bool removeVote) {
        if (!removeVote) {
            voteList.Add(vote);
        } else {
            voteList.Remove(vote);
        }
    }

    [ClientRpc]
    void CountdownUIClient(bool start) {
        startedCountdown = start;
        countdownUI.enabled = start;
        countdownText.enabled = start;
    }

    [Server]
    void OnVoteAdd(int vote) {
        if (voteList.Count > 0) {
            CountdownStart();
        }
    }

    [Server]
    void OnVoteRemove(int vote, PlanetSelect p) {
        if (voteList.Count == 0) {
            CountdownStop();
        }
    }
}
