using UnityEngine;
using Mirror;
using Unity.VisualScripting;
using TMPro;

public class MirrorSampleServer : NetworkBehaviour
{
    [SerializeField] TMP_InputField textfield;
    [SerializeField] TextMeshProUGUI output;
    [SyncVar(hook = nameof(SetOutput))] public string text;

    private void Start() {
        textfield = GameObject.Find("textdebug").GetComponent<TMP_InputField>();
        output = GameObject.Find("PlanetHunterMulti").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isClient) {
            SendText();
        }
        //if (isServer) {
        //    SetOutput();
        //}
    }

    [Command] public void SendText() {
        if (textfield != null) {
            Debug.Log(textfield.text);
            text = textfield.text;
        }
    }

    void SetOutput(string oldValue, string newValue) {
        if (output != null) {
            output.text = newValue;
        }
    }
}
