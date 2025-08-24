using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a "bubble" message above the player, with a background that resizes to fit the text.
/// Attach this script to the bubble UI GameObject only (not the player root).
/// Requires a child TextMeshProUGUI for the message text and an Image for the background.
/// The billboarding is handled separately by a Billboard script on the bubble GameObject.
/// </summary>
public class BubbleMessageUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI messageText; // Assign in inspector
    [SerializeField] private Image backgroundImage;        // Assign in inspector

    [Header("Padding")]
    [SerializeField] private Vector2 padding = new Vector2(24, 10);

    [Header("Bubble Size Constraints (World Units)")]
    [Tooltip("The maximum width of the bubble in world units.")]
    [SerializeField] private float maxWidth = 2f;
    [Tooltip("The maximum height of the bubble in world units.")]
    [SerializeField] private float maxHeight = 0.5f;

    private RectTransform backgroundRect;
    private RectTransform textRect;

    private void Awake()
    {
        if (!messageText) messageText = GetComponentInChildren<TextMeshProUGUI>();
        if (!backgroundImage) backgroundImage = GetComponentInChildren<Image>();
        if (backgroundImage != null) backgroundRect = backgroundImage.rectTransform;
        if (messageText != null) textRect = messageText.rectTransform;
        if (messageText != null)
        {
            // Set ellipsis if not set in inspector
            messageText.overflowMode = TextOverflowModes.Ellipsis;
        }
    }

    /// <summary>
    /// Show a message in the bubble. The background will auto-resize within max width/height.
    /// </summary>
    public void Show(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.ForceMeshUpdate();

            // Calculate preferred values
            Vector2 preferred = messageText.GetPreferredValues(msg, maxWidth, maxHeight);

            // Clamp preferred size to maxWidth/maxHeight
            float width = Mathf.Min(preferred.x, maxWidth);
            float height = Mathf.Min(preferred.y, maxHeight);

            // Set the rects
            if (textRect != null)
                textRect.sizeDelta = new Vector2(width, height);

            if (backgroundRect != null)
                backgroundRect.sizeDelta = new Vector2(width, height) + padding;
        }
        // Only disables/enables the bubble UI, not the parent/player!
        if (backgroundImage != null)
            backgroundImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the bubble message (only disables this bubble UI GameObject).
    /// </summary>
    public void Hide()
    {
        if (backgroundImage != null)
            backgroundImage.gameObject.SetActive(false);
    }
}