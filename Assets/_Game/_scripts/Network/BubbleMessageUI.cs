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

    private RectTransform backgroundRect;
    private RectTransform textRect;

    private void Awake()
    {
        if (!messageText) messageText = GetComponentInChildren<TextMeshProUGUI>();
        if (!backgroundImage) backgroundImage = GetComponentInChildren<Image>();
        if (backgroundImage != null) backgroundRect = backgroundImage.rectTransform;
        if (messageText != null) textRect = messageText.rectTransform;
    }

    /// <summary>
    /// Show a message in the bubble. The background will auto-resize.
    /// </summary>
    public void Show(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.ForceMeshUpdate();
            Vector2 textSize = messageText.GetRenderedValues(false);
            if (backgroundRect != null)
                backgroundRect.sizeDelta = textSize + padding;
        }
        // Only disables/enables the bubble UI, not the parent/player!
        backgroundImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the bubble message (only disables this bubble UI GameObject).
    /// </summary>
    public void Hide()
    {
         backgroundImage.gameObject.SetActive(false);
    }
}