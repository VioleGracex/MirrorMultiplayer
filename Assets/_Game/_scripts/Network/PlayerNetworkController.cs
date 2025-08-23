using Mirror;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles player networking, nickname display, bubble message display,
/// animation sync, and requests server-approved spawning of a cube in front of the player
/// via the ServerSpawnManager. Assumes setup with Mirror and a CharacterController-based movement.
/// The billboarding is handled separately by a Billboard script.
/// </summary>
public class PlayerNetworkController : NetworkBehaviour
{
    [Header("Nickname")]
    [SyncVar(hook = nameof(OnNicknameChanged))]
    public string playerNickname = "Player";
    [SerializeField] private TextMeshProUGUI nicknameBillboardInstance;

    [Header("Bubble Message")]
    [SerializeField] private BubbleMessageUI bubbleMessage; // Assign in inspector (should be the bubble UI, not the player!)

    [Header("Animation")]
    public Animator animator; // Assign in inspector or via GetComponent

    [Header("Cube Spawn")]
    public GameObject cubePrefab; // Assign a cube prefab in the inspector (should also be in ServerSpawnManager's list)
    public float cubeDistance = 2f; // Distance in front of the player to spawn the cube
    public float cubeCheckRadius = 0.5f; // Radius to check for empty space
    public Vector2 cubeSpawnOffset = Vector2.zero; // x: right/left offset, y: up/down offset (in local space)
    public LayerMask cubeCheckLayerMask = ~0; // Default: everything

    Vector3 lastCubeCheckPos;

    void Start()
    {
        // Set initial nickname on billboard
        if (nicknameBillboardInstance)
            nicknameBillboardInstance.text = playerNickname;

        // Only local player shows nickname input UI
        if (isLocalPlayer)
        {
            var inputUI = FindObjectOfType<PlayerNicknameInputUI>();
            if (inputUI) inputUI.AssignPlayer(this);
        }
    }

    void Update()
    {
        // Show bubble message on local input
        if (isLocalPlayer && Input.GetKeyDown(KeyCode.H))
        {
            CmdShowBubbleMessage("Hello !");
        }

        // Animation sync for Mirror
        if (isLocalPlayer && animator)
        {
            float move = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));
            bool isMoving = move > 0.1f;
            CmdUpdateAnim(isMoving);
        }

        // Request to spawn cube in front of player if J is pressed
        if (isLocalPlayer && Input.GetKeyDown(KeyCode.J))
        {
            // Calculate spawn position in local space (forward, right, up)
            Vector3 spawnPos = transform.position
                + transform.forward * cubeDistance
                + transform.right * cubeSpawnOffset.x
                + transform.up * cubeSpawnOffset.y;

            lastCubeCheckPos = spawnPos; // For gizmos

            // Optionally: Check locally before sending request (can skip for trustless, server will always check)
            bool spaceEmpty = !Physics.CheckSphere(spawnPos, cubeCheckRadius, cubeCheckLayerMask, QueryTriggerInteraction.Ignore);

            if (!spaceEmpty)
            {
                Debug.Log("Can't spawn: space is occupied.");
                return;
            }

            // Request the server to spawn the cube via ServerSpawnManager
            if (ServerSpawnManager.Instance != null && cubePrefab != null)
            {
                int prefabIndex = ServerSpawnManager.Instance.spawnablePrefabs.IndexOf(cubePrefab);
                if (prefabIndex != -1)
                    CmdRequestSpawn(prefabIndex, spawnPos, Quaternion.identity, cubeCheckLayerMask.value, cubeCheckRadius);
                else
                    Debug.LogWarning("Cube prefab not registered in ServerSpawnManager's spawnablePrefabs list.");
            }
            else
            {
                Debug.LogWarning("ServerSpawnManager.Instance or cubePrefab is null! Make sure they're assigned.");
            }
        }
        else if (isLocalPlayer)
        {
            // Keep gizmo position up to date for preview
            lastCubeCheckPos = transform.position
                + transform.forward * cubeDistance
                + transform.right * cubeSpawnOffset.x
                + transform.up * cubeSpawnOffset.y;
        }
    }

    #region Nickname

    public void SetNickname(string nickname)
    {
        if (isLocalPlayer)
            CmdSetNickname(nickname);
    }

    [Command]
    void CmdSetNickname(string nickname)
    {
        playerNickname = nickname;
    }

    void OnNicknameChanged(string oldName, string newName)
    {
        if (nicknameBillboardInstance)
            nicknameBillboardInstance.text = newName;
    }

    #endregion

    #region Bubble Message

    [Command]
    void CmdShowBubbleMessage(string msg)
    {
        RpcShowBubbleMessage(msg);
    }

    [ClientRpc]
    void RpcShowBubbleMessage(string msg)
    {
        if (bubbleMessage)
        {
            bubbleMessage.Show(msg);
            CancelInvoke(nameof(HideBubbleMessage));
            Invoke(nameof(HideBubbleMessage), 2f); // Hide after 2 seconds
        }
    }

    void HideBubbleMessage()
    {
        if (bubbleMessage)
            bubbleMessage.Hide();
    }

    #endregion

    #region Animation Sync

    [SyncVar(hook = nameof(OnMoveAnimChanged))]
    public bool animIsMoving;

    [Command]
    void CmdUpdateAnim(bool isMoving)
    {
        animIsMoving = isMoving;
    }

    void OnMoveAnimChanged(bool oldVal, bool newVal)
    {
        if (animator)
            animator.SetBool("run", newVal); // Assumes "run" is the parameter
    }

    #endregion

    #region Cube Spawning

    // Called from Update, requests the server manager to spawn a prefab
    [Command]
    void CmdRequestSpawn(int prefabIndex, Vector3 pos, Quaternion rot, int layerMaskValue, float checkRadius)
    {
        if (ServerSpawnManager.Instance != null)
            ServerSpawnManager.Instance.ServerTrySpawn(prefabIndex, pos, rot, layerMaskValue, checkRadius);
        else
            Debug.LogWarning("No ServerSpawnManager instance found on server.");
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        // Show where the cube check will happen (editor only).
        Gizmos.color = Color.yellow;
        Vector3 checkPos = Application.isPlaying ? lastCubeCheckPos :
            transform.position
            + transform.forward * cubeDistance
            + transform.right * cubeSpawnOffset.x
            + transform.up * cubeSpawnOffset.y;
        Gizmos.DrawWireSphere(checkPos, cubeCheckRadius);

        // Also show forward direction for reference
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, checkPos);
    }

    #endregion
}