using Mirror;
using TMPro;
using UnityEngine;

public class PlayerCounterUI : NetworkBehaviour
{

    [SyncVar(hook = nameof(SetPlayerCountText))] public string playerCountString = "0/4";
    public TextMeshProUGUI text;

    void SetPlayerCountText(string oldValue, string newValue) {
        text.text = newValue;
    }
}
