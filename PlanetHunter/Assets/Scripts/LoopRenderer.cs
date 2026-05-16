using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

public class LoopRenderer : MonoBehaviour {
    [Header("Map Setup")]
    public float worldWidth = 100f;
    public float worldHeight = 100f;

    private SpriteRenderer myRenderer;
    private Material[] loopMaterials;

    void Start() {
        myRenderer = GetComponent<SpriteRenderer>();
        if (myRenderer == null || myRenderer.sharedMaterial == null) return;
        loopMaterials = new Material[9];

        // use 9 offset materials
        for (int i = 0; i < 9; i++) {
            loopMaterials[i] = new Material(myRenderer.sharedMaterial);
            loopMaterials[i].SetFloat("_InstanceIndex", i+1);
        }
        myRenderer.materials = loopMaterials;
    }
}
