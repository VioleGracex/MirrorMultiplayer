using Mirror;
using UnityEngine;
using System.Collections.Generic;

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

        // If request is below ground, move it just above ground before checking space.
        if (position.y < 0)
        {
            // Raycast upwards to find the ground from below
            RaycastHit hit;
            // Assume ground is at layer 0 (Default) - you can adjust this as needed
            if (Physics.Raycast(new Vector3(position.x, 1000f, position.z), Vector3.down, out hit, Mathf.Infinity))
            {
                // Place slightly above hit point
                position.y = hit.point.y + checkRadius + 0.05f;
            }
            else
            {
                // If no ground found, fallback to y=0.5
                position.y = checkRadius + 0.05f;
            }
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