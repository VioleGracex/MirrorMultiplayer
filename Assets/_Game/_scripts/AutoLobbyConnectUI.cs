using Mirror;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI for auto-connect to lobby (host/client) with Mirror.
/// Attach to Canvas, assign buttons.
/// </summary>
public class AutoLobbyConnectUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public NetworkManager manager;

    void Start()
    {
        hostButton.onClick.AddListener(OnHost);
        clientButton.onClick.AddListener(OnClient);
    }

    void OnHost()
    {
        manager.StartHost();
    }

    void OnClient()
    {
        manager.StartClient();
    }
}