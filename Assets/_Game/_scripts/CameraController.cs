using UnityEngine;

/// <summary>
/// Camera movement script for third person games.
/// Attach this script to a scene object (the "camera rig").
/// The MainCamera should be a child of this object in the scene.
/// This script will move/rotate the rig to follow the local player.
/// Press 'C' to toggle cursor lock/hide.
/// When cursor is shown, camera follows player but does not rotate with mouse.
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

    [SerializeField] private Camera cam;

    private Transform player;
    private float mouseX;
    private float mouseY;
    private bool isCursorLocked = true;

    void Start()
    {
        if (!cam)
            cam = Camera.main;

        SetCursorLock(false);

        if (cam)
        {
            cam.transform.localPosition = cameraLocalOffset;
            cam.transform.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            SetCursorLock(!isCursorLocked);

        if (player == null)
            FindLocalPlayer();

        if (player == null)
            return;

        // Always follow player position even if cursor is visible
        transform.position = player.position;

        if (canZoom && cam != null && Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            cam.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * sensitivity * 2;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, 20f, 80f);
        }

        // Only rotate camera with mouse if cursor is locked
        if (isCursorLocked)
        {
            if (clickToMoveCamera && Input.GetAxisRaw("Fire2") == 0)
                return;

            mouseX += Input.GetAxis("Mouse X") * sensitivity;
            mouseY += Input.GetAxis("Mouse Y") * sensitivity;
            mouseY = Mathf.Clamp(mouseY, cameraLimit.x, cameraLimit.y);

            transform.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
        }
        // If cursor is not locked, do not change rotation (camera will keep last rotation)
    }

    private void SetCursorLock(bool locked)
    {
        isCursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void FindLocalPlayer()
    {
        foreach (var p in FindObjectsOfType<PlayerNetworkController>())
        {
            if (p.isLocalPlayer)
            {
                player = p.transform;
                SetCursorLock(true);
                break;
            }
        }
    }

    /// <summary>
    /// Allows runtime assignment of the player to follow.
    /// </summary>
    public void SetFollowTarget(Transform target)
    {
        player = target;
    }
}