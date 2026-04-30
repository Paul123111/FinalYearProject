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

    private void Awake() {
        myRenderer = GetComponent<SpriteRenderer>();
        propBlock = new MaterialPropertyBlock();
    }

    void OnColourChanged(long oldValue, long newValue) {
        colourScheme = PlayerColourSchemes.GetColorScheme(newValue);
        myRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_NewColour", colourScheme[0]);
        propBlock.SetColor("_NewOutline", colourScheme[1]);
        myRenderer.SetPropertyBlock(propBlock);
        Debug.Log("Colour Changed");
    }
}

// Colour schemes have two colours
//   - index 0: a fill colour
//   - index 1: an outline colour
public static class PlayerColourSchemes {
    public static readonly Dictionary<long, string[]> hexMap = new Dictionary<long, string[]> {
        { 1, new string[] { "#009cfc", "#0c08ec" } }, // blue, dark blue
        { 2, new string[] { "#e41800", "#680008" } }, // red, dark red
        { 3, new string[] { "#24d000", "#004c00" } }, // green, dark green
        { 4, new string[] { "#fcc400", "#884c00" } }, // yellow, light brown
    };
    public static Color[] GetColorScheme(long playerNum) {
        string[] hexes;
        Color[] colours = new Color[2];
        Debug.Log(playerNum);
        if (hexMap.TryGetValue(playerNum, out hexes)) {
            for (int i = 0; i < colours.Length; i++) {
                if (hexes.Length == 2 && ColorUtility.TryParseHtmlString(hexes[i], out Color color)) {
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
