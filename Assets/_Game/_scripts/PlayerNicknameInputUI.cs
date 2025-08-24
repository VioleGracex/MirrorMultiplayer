using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles nickname input and submit UI for local player.
/// Attach to Canvas in scene, assign TMP_InputField and Button.
/// Ensures all nicknames are unique.
/// </summary>
public class PlayerNicknameInputUI : MonoBehaviour
{
    public static PlayerNicknameInputUI Instance { get; private set; }
    public TMP_InputField inputField;
    public Button submitButton;
    private readonly List<PlayerNetworkController> players = new List<PlayerNetworkController>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        submitButton.onClick.AddListener(OnSubmit);
    }

    public void RegisterPlayer(PlayerNetworkController player)
    {
        if (!players.Contains(player))
            players.Add(player);
    }

    public void UnregisterPlayer(PlayerNetworkController player)
    {
        players.Remove(player);
    }

    public IReadOnlyList<PlayerNetworkController> GetAllPlayers() => players.AsReadOnly();

    /// <summary>
    /// Finds the local player from the registered players list.
    /// </summary>
    public PlayerNetworkController FindLocalPlayer()
    {
        foreach (var p in players)
        {
            if (p != null && p.isLocalPlayer)
                return p;
        }
        return null;
    }

    /// <summary>
    /// Checks if the nickname is unique among all players (case-insensitive).
    /// </summary>
    private bool IsNicknameUnique(string nickname)
    {
        string lowerNick = nickname.Trim().ToLowerInvariant();
        foreach (var p in players)
        {
            if (p != null && !string.IsNullOrEmpty(p.playerNickname) &&
                p.playerNickname.Trim().ToLowerInvariant() == lowerNick)
                return false;
        }
        return true;
    }

    void OnSubmit()
    {
        Debug.Log("[PlayerNicknameInputUI] OnSubmit clicked");
        var localPlayer = FindLocalPlayer();
        string newNick = inputField.text?.Trim();

        if (localPlayer == null || string.IsNullOrWhiteSpace(newNick))
            return;

        if (!IsNicknameUnique(newNick))
        {
            Debug.LogWarning($"Nickname '{newNick}' is already taken.");
            // Optionally, show feedback in UI:
            if (inputField != null)
                inputField.text = "";
            inputField.placeholder.GetComponent<TMP_Text>().text = "Name already taken!";
            return;
        }

        localPlayer.SetNickname(newNick);
    }
}