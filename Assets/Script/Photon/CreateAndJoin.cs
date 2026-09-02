using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateAndJoin : MonoBehaviourPunCallbacks
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField createRoomInput;
    [SerializeField] private TMP_InputField joinRoomInput;


    [Header("Buttons")]
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;


    [Header("Settings")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private byte maxPlayers = 2;


    private bool _isWaitingForRoom;


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        UpdateButtons();

        Debug.Log(
            "Photon State: " +
            PhotonNetwork.NetworkClientState
        );
    }


    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        UpdateButtons();
    }


    // =====================================================
    // PHOTON READY CHECK
    // =====================================================

    private bool IsPhotonReady()
    {
        return PhotonNetwork.IsConnectedAndReady &&
               PhotonNetwork.Server == ServerConnection.MasterServer;
    }


    // =====================================================
    // CREATE ROOM
    // =====================================================

    public void CreateRoom()
    {
        if (!IsPhotonReady())
        {
            Debug.LogWarning(
                "Photon is not ready yet.\n" +
                "Current Server: " +
                PhotonNetwork.Server
            );

            return;
        }


        string playerName = playerNameInput.text.Trim();


        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Please enter a player name.");

            return;
        }


        string roomName = createRoomInput.text.Trim();


        // Generate room name if empty.
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room_" + Random.Range(1000, 9999);
        }


        // Set Photon player name.
        PhotonNetwork.NickName = playerName;


        RoomOptions roomOptions = new RoomOptions();


        roomOptions.MaxPlayers = maxPlayers;
        
        roomOptions.IsVisible = true;
        
        roomOptions.IsOpen = true;


        Debug.Log("Creating Room: " + roomName);
        
        _isWaitingForRoom = true;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }


    // =====================================================
    // JOIN ROOM
    // =====================================================

    public void JoinRoom()
    {
        if (!IsPhotonReady())
        {
            Debug.LogWarning(
                "Photon is not ready yet.\n" +
                "Current Server: " +
                PhotonNetwork.Server
            );

            return;
        }


        string playerName = playerNameInput.text.Trim();


        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Please enter a player name.");
            return;
        }
        
        string roomName = joinRoomInput.text.Trim();
        
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("Please enter a Room Code.");
            return;
        }


        // Set player name.
        PhotonNetwork.NickName = playerName;
        
        Debug.Log(
            "Joining Room: " +
            roomName
        );

        _isWaitingForRoom = true;


        PhotonNetwork.JoinRoom(
            roomName
        );
    }


    // =====================================================
    // JOINED ROOM
    // =====================================================

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room Successfully!");


        Debug.Log("Room Name: " + PhotonNetwork.CurrentRoom.Name);


        Debug.Log("Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount);
        
        _isWaitingForRoom =
            false;

        // =================================================
        // IMPORTANT
        //
        // IMMEDIATELY LOAD GAME
        //
        // DO NOT WAIT FOR SECOND PLAYER
        // =================================================

        PhotonNetwork.LoadLevel(gameSceneName);
    }
    
    // =====================================================
    // CREATE ROOM FAILED
    // =====================================================

    public override void OnCreateRoomFailed(
        short returnCode,
        string message)
    {
        _isWaitingForRoom =
            false;


        Debug.LogError(
            "Create Room Failed!\n" +
            "Code: " +
            returnCode +
            "\nMessage: " +
            message
        );
    }


    // =====================================================
    // JOIN ROOM FAILED
    // =====================================================

    public override void OnJoinRoomFailed(
        short returnCode,
        string message)
    {
        _isWaitingForRoom =
            false;


        Debug.LogError(
            "Join Room Failed!\n" +
            "Code: " +
            returnCode +
            "\nMessage: " +
            message
        );
    }


    // =====================================================
    // BUTTON STATE
    // =====================================================

    private void UpdateButtons()
    {
        bool canUsePhoton =
            IsPhotonReady();


        if (createButton != null)
        {
            createButton.interactable =
                canUsePhoton &&
                !_isWaitingForRoom;
        }


        if (joinButton != null)
        {
            joinButton.interactable =
                canUsePhoton &&
                !_isWaitingForRoom;
        }
    }
}