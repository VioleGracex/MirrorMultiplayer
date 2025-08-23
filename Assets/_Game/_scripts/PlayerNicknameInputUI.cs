using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles nickname input and submit UI for local player.
/// Attach to Canvas in scene, assign TMP_InputField and Button.
/// </summary>
public class PlayerNicknameInputUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button submitButton;
    private PlayerNetworkController player;

    void Awake()
    {
        submitButton.onClick.AddListener(OnSubmit);
    }

    public void AssignPlayer(PlayerNetworkController p)
    {
        player = p;
        gameObject.SetActive(true);
    }

    void OnSubmit()
    {
        if (player && !string.IsNullOrWhiteSpace(inputField.text))
            player.SetNickname(inputField.text);
        gameObject.SetActive(false);
    }
}