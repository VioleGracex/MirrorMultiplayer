using UnityEngine;

/// <summary>
/// Camera movement script for third person games.
/// This Script should not be applied to the camera! It is attached to an empty object,
/// and inside it (as a child object) should be your game's MainCamera.
/// The script will automatically parent the MainCamera if it's not already a child,
/// and will handle following and rotating around the player.
/// Press 'C' to toggle cursor lock/hide.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Tooltip("Enable to move the camera by holding the right mouse button. Does not work with joysticks.")]
    public bool clickToMoveCamera = false;
    [Tooltip("Enable zoom in/out when scrolling the mouse wheel. Does not work with joysticks.")]
    public bool canZoom = true;
    [Space]
    [Tooltip("The higher it is, the faster the camera moves. It is recommended to increase this value for games that uses joystick.")]
    public float sensitivity = 5f;

    [Tooltip("Camera Y rotation limits. The X axis is the maximum it can go up and the Y axis is the maximum it can go down.")]
    public Vector2 cameraLimit = new Vector2(-45, 40);

    [Tooltip("Camera offset relative to this GameObject (e.g. new Vector3(0, 5, -8) for classic third person).")]
    public Vector3 cameraLocalOffset = new Vector3(0, 5, -8);

    float mouseX;
    float mouseY;
    float offsetDistanceY;

    [SerializeField] private Transform player;
    [SerializeField] private Camera cam;

    // Track cursor lock state
    private bool isCursorLocked = true;

    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("CameraController: No GameObject with tag 'Player' found. Camera will not follow.");
            }
        }

        // Find and assign camera if not set
        if (!cam)
            cam = Camera.main;

        // If the camera is not already a child, parent and reset offset
        if (cam != null && cam.transform.parent != transform)
        {
            cam.transform.SetParent(transform);
            cam.transform.localPosition = cameraLocalOffset;
            cam.transform.localRotation = Quaternion.identity;
        }

        offsetDistanceY = transform.position.y;

        // Start with cursor locked and hidden
        SetCursorLock(true);
    }

    void Update()
    {
        if (player == null)
            return;

        // Toggle cursor lock/hide with C
        if (Input.GetKeyDown(KeyCode.C))
        {
            SetCursorLock(!isCursorLocked);
        }

        // Only rotate/move camera if cursor is locked
        if (isCursorLocked)
        {
            // Follow player - camera rig offset
            transform.position = player.position + new Vector3(0, offsetDistanceY, 0);

            // Set camera zoom when mouse wheel is scrolled
            if (canZoom && cam != null && Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                cam.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * sensitivity * 2;
                // Clamp field of view for reasonable zoom range
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, 20f, 80f);
            }

            // Checker for right click to move camera
            if (clickToMoveCamera && Input.GetAxisRaw("Fire2") == 0)
                return;

            // Calculate new rotation
            mouseX += Input.GetAxis("Mouse X") * sensitivity;
            mouseY += Input.GetAxis("Mouse Y") * sensitivity;
            // Apply camera vertical limits
            mouseY = Mathf.Clamp(mouseY, cameraLimit.x, cameraLimit.y);

            // Rotate the rig, camera as child will follow
            transform.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
        }
    }

    private void SetCursorLock(bool locked)
    {
        isCursorLocked = locked;
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}