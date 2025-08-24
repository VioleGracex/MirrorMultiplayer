using Mirror;
using TMPro;
using UnityEngine;

// Handles player networking, nickname display, bubble message display, animation sync, and cube spawning.
public class PlayerNetworkController : NetworkBehaviour
{

    #region Inspector Fields
    [Header("Nickname")]
    [SyncVar(hook = nameof(OnNicknameChanged))]
    public string playerNickname = "Player";
    [SerializeField] private TextMeshProUGUI nicknameBillboardInstance;

    [Header("Bubble Message")]
    [SerializeField] private BubbleMessageUI bubbleMessage;

    [Header("Animation")]
    public Animator animator;

    [Header("Cube Spawn")]
    public GameObject cubePrefab;
    public float cubeDistance = 2f;
    public float cubeCheckRadius = 0.5f;
    public Vector2 cubeSpawnOffset = Vector2.zero;
    public LayerMask cubeCheckLayerMask = ~0;
    #endregion

    #region Private State
    private Vector3 lastCubeCheckPos;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        // Ensure nickname billboard is assigned
        if (nicknameBillboardInstance == null)
            nicknameBillboardInstance = GetComponentInChildren<TextMeshProUGUI>(true);

        // Register with nickname UI
        PlayerNicknameInputUI.Instance?.RegisterPlayer(this);
    }

    void OnDestroy()
    {
        // Unregister from nickname UI
        PlayerNicknameInputUI.Instance?.UnregisterPlayer(this);
    }

    public override void OnStartLocalPlayer()
    {
        // Assign a default unique nickname for the local player
        CmdSetNickname($"Player_{netId}");
    }

    void Start()
    {
        // Update nickname display for late joiners
        if (nicknameBillboardInstance)
            nicknameBillboardInstance.text = playerNickname;
    }

    void Update()
    {
        // Only allow local player to process input
        if (!isLocalPlayer) return;

        HandleBubbleMessageInput();
        HandleAnimationInput();
        HandleCubeSpawnInput();
    }
    #endregion

    #region Nickname Logic
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
        if (nicknameBillboardInstance == null)
            nicknameBillboardInstance = GetComponentInChildren<TextMeshProUGUI>(true);
        if (nicknameBillboardInstance)
            nicknameBillboardInstance.text = newName;
    }
    #endregion

    #region Bubble Message Logic
    void HandleBubbleMessageInput()
    {
        if (Input.GetKeyDown(KeyCode.H))
            CmdShowBubbleMessage("Hello !");
    }

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

    #region Animation Logic
    [SyncVar(hook = nameof(OnMoveAnimChanged))]
    public bool animIsMoving;

    void HandleAnimationInput()
    {
        if (animator && !Cursor.visible)
        {
            float move = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));
            bool isMoving = move > 0.1f;
            CmdUpdateAnim(isMoving);
        }
    }

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

    #region Cube Spawning Logic
    void HandleCubeSpawnInput()
    {
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