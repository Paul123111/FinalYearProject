using Mirror;
using UnityEngine;

public class LoopRenderer : NetworkBehaviour {
    [Header("Map Setup")]
    public float worldWidth = 100f;
    public float worldHeight = 100f;

    private SpriteRenderer myRenderer;
    private Material[] loopMaterials;
    Bounds massiveBounds;
    bool isInitialised = false;

    void Awake() {
#if UNITY_SERVER
        Destroy(this);
        return;
#endif
    }

    public override void OnStartClient() {
        base.OnStartClient();
        myRenderer = GetComponent<SpriteRenderer>();
        if (myRenderer == null || myRenderer.sharedMaterial == null) return;
        loopMaterials = new Material[9];

        // use 9 offset materials
        for (int i = 0; i < 9; i++) {
            loopMaterials[i] = new Material(myRenderer.sharedMaterial);
            loopMaterials[i].SetFloat("_InstanceIndex", i+1);
        }
        myRenderer.materials = loopMaterials;
        massiveBounds = new Bounds(Vector3.zero, new Vector3(10000f, 10000f, 10000f));
        isInitialised = true;
    }

    void LateUpdate() {
        // Only run if the client setup has completed
        if (!isInitialised || myRenderer == null) return;

        // Overwrites network transform syncs and prevents culling every frame
        myRenderer.localBounds = massiveBounds;
    }
}
