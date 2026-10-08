using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomGameMode : MonoBehaviourPunCallbacks
{
    public enum GameMode
    {
        Solo,
        Multiplayer
    }

    [Header("Buttons")]
    [SerializeField] private Button playSoloButton;
    [SerializeField] private Button multiplayerButton;

    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private string lobbySceneName = "Lobby";

    private bool _isCreatingRoom;

    private void Start()
    {
        UpdateButtonState();
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private bool IsPhotonReady()
    {
        return PhotonNetwork.IsConnected &&
               PhotonNetwork.IsConnectedAndReady &&
               PhotonNetwork.Server == ServerConnection.MasterServer;
    }

    private void UpdateButtonState()
    {
        bool ready = IsPhotonReady() && !_isCreatingRoom;

        if (playSoloButton != null)
            playSoloButton.interactable = ready;

        if (multiplayerButton != null)
            multiplayerButton.interactable = ready;
    }

    // =========================================================
    // SOLO
    // =========================================================

    public void PlaySolo()
    {
        if (!IsPhotonReady())
        {
            Debug.LogWarning("[MENU] Photon is not ready.");
            return;
        }

        Debug.Log("[MENU] Play Solo");

        CreateSoloRoom();
    }

    private void CreateSoloRoom()
    {
        if (_isCreatingRoom)
            return;

        _isCreatingRoom = true;

        string roomName = "Solo_" + Random.Range(1000, 9999);

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 1,
            IsVisible = false,
            IsOpen = true,

            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "GameMode", "Solo" }
                }
        };

        Debug.Log("[MENU] Creating SOLO room: " + roomName);

        PhotonNetwork.CreateRoom(roomName, options);
    }

    // =========================================================
    // MULTIPLAYER
    // =========================================================

    public void OpenMultiplayerLobby()
    {
        Debug.Log("[MENU] Multiplayer selected.");

        SceneManager.LoadScene(lobbySceneName);
    }

    // =========================================================
    // SOLO ROOM JOINED
    // =========================================================

    public override void OnJoinedRoom()
    {
        Debug.Log("[MENU] Joined Solo Room: " + PhotonNetwork.CurrentRoom.Name);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    // =========================================================
    // CREATE FAILED
    // =========================================================

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _isCreatingRoom = false;

        Debug.LogError("[MENU] Solo room creation failed: " + returnCode + " | " + message);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[MENU] Connected to Photon Master.");
        UpdateButtonState();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        _isCreatingRoom = false;

        Debug.LogError("[MENU] Photon disconnected: " + cause);
    }
}