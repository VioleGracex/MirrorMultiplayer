using Mirror;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles player networking, nickname display, bubble message display,
/// animation sync, and requests server-approved spawning of a cube in front of the player.
/// The billboarding is handled separately by a Billboard script.
/// </summary>
public class PlayerNetworkController : NetworkBehaviour
{
    [Header("Nickname")]
    [SyncVar(hook = nameof(OnNicknameChanged))]
    public string playerNickname = "Player";
    [SerializeField] private TextMeshProUGUI nicknameBillboardInstance;

    [Header("Bubble Message")]
    [SerializeField] private BubbleMessageUI bubbleMessage; // Assign in inspector

    [Header("Animation")]
    public Animator animator; // Assign in inspector or via GetComponent

    [Header("Cube Spawn")]
    public GameObject cubePrefab;
    public float cubeDistance = 2f;
    public float cubeCheckRadius = 0.5f;
    public Vector2 cubeSpawnOffset = Vector2.zero;
    public LayerMask cubeCheckLayerMask = ~0;

    Vector3 lastCubeCheckPos;

   void Awake()
    {
        if (nicknameBillboardInstance == null)
            nicknameBillboardInstance = GetComponentInChildren<TextMeshProUGUI>(true);

        PlayerNicknameInputUI.Instance?.RegisterPlayer(this);
    }
    void OnDestroy()
    {
        // Remove self from UI player list if possible
        PlayerNicknameInputUI.Instance?.UnregisterPlayer(this);
    }
    public override void OnStartLocalPlayer()
    {
        CmdSetNickname("Player_" + netId);
    }

    void Start()
    {
        // Always update nickname for late joiners
        if (nicknameBillboardInstance)
            nicknameBillboardInstance.text = playerNickname;
    }

    void Update()
    {
        // Only allow local player to process input
        if (!isLocalPlayer) return;

        // Show bubble message on local input
        if (Input.GetKeyDown(KeyCode.H))
        {
            CmdShowBubbleMessage("Hello !");
        }

        // Animation sync for Mirror (run param only set if not paused/menu)
        if (animator && !Cursor.visible)
        {
            float move = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));
            bool isMoving = move > 0.1f;
            CmdUpdateAnim(isMoving);
        }

        // Request to spawn cube in front of player if F is pressed
        if (Input.GetKeyDown(KeyCode.F))
        {
            Vector3 spawnPos = transform.position
                + transform.forward * cubeDistance
                + transform.right * cubeSpawnOffset.x
                + transform.up * cubeSpawnOffset.y;

            lastCubeCheckPos = spawnPos;

            bool spaceEmpty = !Physics.CheckSphere(spawnPos, cubeCheckRadius, cubeCheckLayerMask, QueryTriggerInteraction.Ignore);

            if (!spaceEmpty)
            {
                Debug.Log("Can't spawn: space is occupied.");
                return;
            }

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
        else
        {
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
        // Always try to find the TextMeshProUGUI if not set (handles remote clients and late join)
        if (nicknameBillboardInstance == null)
            nicknameBillboardInstance = GetComponentInChildren<TextMeshProUGUI>(true);

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
            Invoke(nameof(HideBubbleMessage), 2f);
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
            animator.SetBool("run", newVal);
    }

    #endregion

    #region Cube Spawning

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
        Gizmos.color = Color.yellow;
        Vector3 checkPos = Application.isPlaying ? lastCubeCheckPos :
            transform.position
            + transform.forward * cubeDistance
            + transform.right * cubeSpawnOffset.x
            + transform.up * cubeSpawnOffset.y;
        Gizmos.DrawWireSphere(checkPos, cubeCheckRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, checkPos);
    }

    #endregion
}