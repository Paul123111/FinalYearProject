using UnityEngine;
using UnityEngine.Pool;
using Unity.Profiling;

// Code is a modified version of PoolController2D from Mobile Game Development labs to 
// support generic object pooling

// Generic base pool wrapper that will be extended to item pools, enemy pools etc.
// Functionality for resetting in object class

[DisallowMultipleComponent] // Only one PoolController2D per GameObject
public abstract class PoolController2D<T> : MonoBehaviour where T : MonoBehaviour, IPoolObject {
    [Header("References")]
    [SerializeField] protected Camera targetCamera;           // If null -> Camera.main
    [SerializeField] protected T poolObject; // possible Objects

    [Header("Pool Sizes")]
    [SerializeField] protected int defaultCapacity = 500;
    [SerializeField] protected int maxSize = 5000;
    [SerializeField] protected int prewarmCount = 1000;

    // Profiling markers (compiled out of non-Development builds)
    static readonly ProfilerMarker k_GetMarker = new ProfilerMarker("Pool.Get");
    static readonly ProfilerMarker k_ReleaseMarker = new ProfilerMarker("Pool.Release");
    static readonly ProfilerMarker k_PrewarmMarker = new ProfilerMarker("Pool.Prewarm");
    static readonly ProfilerMarker k_CreateItemMarker = new ProfilerMarker("Pool.CreateItem");

    // Current active mover count (for HUD display)
    public int ActiveCount => _pool?.CountActive ?? 0;

    protected ObjectPool<T> _pool; // The object pool
    protected float _spawnBudget;            // Accumulated spawn budget (can be fractional, used to smooth spawning)

    void Awake() {
        if (!targetCamera) targetCamera = Camera.main;

        _pool = new ObjectPool<T>(
            createFunc: CreateItem,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyItem,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        // Profile the entire spawning batch to measure total frame impact
        using (k_PrewarmMarker.Auto()) {        // Prewarm (create & immediately release)
            int toPrewarm = Mathf.Clamp(prewarmCount, 0, maxSize);
            for (int i = 0; i < toPrewarm; i++) {
                var item = _pool.Get();
                _pool.Release(item);
            }
        }
    }

    // Spawn a single mover at a random position inside the viewport (with inset)
    protected T SpawnOne() {
        using (k_GetMarker.Auto()) {
            var obj = _pool.Get();
            //obj.ActivateAt(RandomWorldPosOutsideViewport(0.2f));
            return obj;
        }
    }

    // Pool create function
    protected virtual T CreateItem() {
        using (k_CreateItemMarker.Auto()) {
            var inst = Instantiate(poolObject);
            inst.gameObject.SetActive(false);
            return inst;
        }
    }

    // Pool get function
    void OnGet(T obj) => obj.gameObject.SetActive(true);

    // Pool release function
    void OnRelease(T obj) {
        using (k_ReleaseMarker.Auto()) {
            obj.gameObject.SetActive(false);
        }
    }

    // Pool destroy function
    void OnDestroyItem(T obj) {
        if (obj) Destroy(obj.gameObject);
    } 
}

