using Mirror;
using UnityEngine;

/// <summary>
/// Third person movement, only processes input for the local player.
/// Even if the cursor is visible or window focus is lost, gravity is always applied,
/// so the player will keep falling. Only player input and animation parameters for movement are disabled
/// when the cursor is visible, but falling/landing/jump animations can finish so you never get stuck in-air.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : NetworkBehaviour
{
    public float velocity = 5f;
    public float sprintAdittion = 3.5f;
    public float jumpForce = 18f;
    public float jumpTime = 0.85f;
    [Space]
    public float gravity = 9.8f;

    float jumpElapsedTime = 0;
    bool isJumping = false;
    bool isSprinting = false;
    bool isCrouching = false;

    float inputHorizontal;
    float inputVertical;
    bool inputJump;
    bool inputCrouch;
    bool inputSprint;

    Animator animator;
    CharacterController cc;

    // Store previous movement input so we don't move when input is blocked
    float cachedInputHorizontal = 0f;
    float cachedInputVertical = 0f;
    bool cachedIsSprinting = false;
    bool cachedIsCrouching = false;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Only allow the local player to control this character
        if (!isLocalPlayer)
        {
            enabled = false;
            return;
        }

        if (animator == null)
            Debug.LogWarning("Animator component missing on player.");
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Cursor.visible)
        {
            // Prevent player input and movement animations, but allow fall/land/jump to proceed.
            inputHorizontal = 0f;
            inputVertical = 0f;
            inputJump = false;
            inputSprint = false;
            inputCrouch = false;

            // Still update air/land/jump animation state so they can finish naturally
            if (animator != null)
            {
                animator.SetBool("air", cc.isGrounded == false);
                animator.SetBool("run", false); // Ensure run is false when movement is disabled
                // If we just landed, ensure ground state is reflected
            }

            // Do not update run/crouch/sprint/crouch
            return;
        }

        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f;
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl);

        if (inputCrouch)
            isCrouching = !isCrouching;

        // Only set movement/idle animations if not in menu
        if (cc.isGrounded && animator != null)
        {
            animator.SetBool("crouch", isCrouching);

            float minimumSpeed = 0.9f;
            animator.SetBool("run", cc.velocity.magnitude > minimumSpeed);

            isSprinting = cc.velocity.magnitude > minimumSpeed && inputSprint;
            animator.SetBool("sprint", isSprinting);
        }

        if (animator != null)
            animator.SetBool("air", cc.isGrounded == false);

        if (inputJump && cc.isGrounded)
        {
            isJumping = true;
        }

        // Cache movement state for use in FixedUpdate
        cachedInputHorizontal = inputHorizontal;
        cachedInputVertical = inputVertical;
        cachedIsSprinting = isSprinting;
        cachedIsCrouching = isCrouching;

        HeadHittingDetect();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        // Gravity and cc.Move always apply, even if cursor is visible.
        float moveInputHorizontal = Cursor.visible ? 0f : cachedInputHorizontal;
        float moveInputVertical = Cursor.visible ? 0f : cachedInputVertical;
        bool moveIsSprinting = Cursor.visible ? false : cachedIsSprinting;
        bool moveIsCrouching = Cursor.visible ? false : cachedIsCrouching;

        float velocityAdittion = 0;
        if (moveIsSprinting)
            velocityAdittion = sprintAdittion;
        if (moveIsCrouching)
            velocityAdittion = -(velocity * 0.50f);

        float directionX = moveInputHorizontal * (velocity + velocityAdittion) * Time.deltaTime;
        float directionZ = moveInputVertical * (velocity + velocityAdittion) * Time.deltaTime;
        float directionY = 0;

        if (isJumping)
        {
            directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Time.deltaTime;
            jumpElapsedTime += Time.deltaTime;
            if (jumpElapsedTime >= jumpTime)
            {
                isJumping = false;
                jumpElapsedTime = 0;
            }
        }

        // Gravity always applies
        directionY = directionY - gravity * Time.deltaTime;

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        forward = forward * directionZ;
        right = right * directionX;

        // Only rotate if the input is not blocked
        if ((directionX != 0 || directionZ != 0) && !Cursor.visible)
        {
            float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }

        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 horizontalDirection = forward + right;

        Vector3 movement = verticalDirection + horizontalDirection;
        cc.Move(movement);
    }

    void HeadHittingDetect()
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistance;

        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
        {
            jumpElapsedTime = 0;
            isJumping = false;
        }
    }
}