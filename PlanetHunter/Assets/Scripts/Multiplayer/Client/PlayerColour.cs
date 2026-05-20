using Mirror;
using Mirror.BouncyCastle.Bcpg;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColour : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnColourChanged))] public long playerNum = -1;

    SpriteRenderer myRenderer;
    MaterialPropertyBlock propBlock;
    Color[] colourScheme;

    // only need two playerColour types
    [SerializeField] bool isRocket;
    Dictionary<long, string[]> hexMap;

    private void Awake() {
        myRenderer = GetComponent<SpriteRenderer>();
        propBlock = new MaterialPropertyBlock();
        hexMap = PlayerColourSchemes.entityHexMap;
        if (isRocket) {
            hexMap = PlayerColourSchemes.rocketHexMap;
        }
    }

    void OnColourChanged(long oldValue, long newValue) {
        colourScheme = PlayerColourSchemes.GetColorScheme(newValue, hexMap);
        myRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_NewColour", colourScheme[0]);
        propBlock.SetColor("_NewOutline", colourScheme[1]);
        if (colourScheme.Length >= 3) {
            propBlock.SetColor("_NewColour2", colourScheme[2]);
        }
        myRenderer.SetPropertyBlock(propBlock);
        Debug.Log("Colour Changed");
    }
}

// Colour schemes have two or three colours
//   - index 0: a fill colour
//   - index 1: an outline colour
//   - index 2: a face colour (for astronaut)
public static class PlayerColourSchemes {
    public static readonly Dictionary<long, string[]> rocketHexMap = new Dictionary<long, string[]> {
        { 1, new string[] { "#009cfc", "#0c08ec" } }, // blue, dark blue
        { 2, new string[] { "#e41800", "#680008" } }, // red, dark red
        { 3, new string[] { "#24d000", "#004c00" } }, // green, dark green
        { 4, new string[] { "#fcc400", "#884c00" } }, // yellow, light brown
    };
    public static readonly Dictionary<long, string[]> entityHexMap = new Dictionary<long, string[]> {
        { -1, new string[] { "#595959", "#000000", "#D4D4D4" } }, // default
        { 0, new string[] { "#595959", "#000000", "#D4D4D4" } }, // default
        { 1, new string[] { "#0098fc", "#000060", "#7cfcfc" } }, // blue, dark blue, cyan
        { 2, new string[] { "#e41800", "#480014", "#fc2800" } }, // red, dark red, red
        { 3, new string[] { "#24d000", "#002400", "#c0f800" } }, // green, dark green, light green
        { 4, new string[] { "#fcc400", "#602400", "#fce94f" } }, // yellow, light brown, light yellow
    };
    public static Color[] GetColorScheme(long playerNum, Dictionary<long, string[]> hexMap) {
        int len = hexMap[1].Length;
        Debug.Log(len);
        string[] hexes;
        Color[] colours = new Color[len];
        if (hexMap.TryGetValue(playerNum, out hexes)) {
            for (int i = 0; i < colours.Length; i++) {
                if (hexes.Length == len && ColorUtility.TryParseHtmlString(hexes[i], out Color color)) {
                    colours[i] = color;
                } else {
                    Debug.LogError("Colour was not found or hexMap did not return exactly two hexes");
                    colours[i] = Color.magenta;
                }
            }
        }
        Debug.Log(hexes);
        return colours;
    }
}
