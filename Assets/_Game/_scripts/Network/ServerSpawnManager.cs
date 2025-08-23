using Mirror;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkIdentity))]
public class ServerSpawnManager : NetworkBehaviour
{
    public static ServerSpawnManager Instance { get; private set; }
    [Tooltip("List of prefabs that can be spawned via this manager.")]
    public List<GameObject> spawnablePrefabs = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    /// <summary>
    /// Only called on server!
    /// </summary>
    /// <param name="prefabIndex">Index in spawnablePrefabs</param>
    /// <param name="position">World position to spawn at</param>
    /// <param name="rotation">World rotation</param>
    /// <param name="layerMaskValue">LayerMask for Physics check</param>
    /// <param name="checkRadius">Radius for empty space check</param>
    public void ServerTrySpawn(int prefabIndex, Vector3 position, Quaternion rotation, int layerMaskValue, float checkRadius)
    {
        if (prefabIndex < 0 || prefabIndex >= spawnablePrefabs.Count)
        {
            Debug.LogWarning("Invalid prefab index requested.");
            return;
        }

        var prefab = spawnablePrefabs[prefabIndex];

        // Approval logic: No spawn below ground
        if (position.y < 0)
        {
            Debug.LogWarning("Denied spawn below ground.");
            return;
        }

        // Approval logic: Check if space is empty using Physics.CheckSphere
        LayerMask layerMask = layerMaskValue;
        bool spaceIsEmpty = !Physics.CheckSphere(position, checkRadius, layerMask, QueryTriggerInteraction.Ignore);
        if (!spaceIsEmpty)
        {
            Debug.LogWarning($"Denied spawn: space at {position} (r={checkRadius}) is occupied for layers {layerMask}.");
            return;
        }

        var obj = Instantiate(prefab, position, rotation);
        NetworkServer.Spawn(obj);
    }
}