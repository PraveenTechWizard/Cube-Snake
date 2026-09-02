using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [Header("Scene")]
    [SerializeField] private string lobbySceneName = "Lobby";

    private bool _isLoadingLobby;


    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        ConnectToPhoton();
    }


    // =====================================================
    // CONNECT
    // =====================================================

    private void ConnectToPhoton()
    {
        Debug.Log(
            $"Connected: {PhotonNetwork.IsConnected} | " +
            $"Server: {PhotonNetwork.Server} | " +
            $"InRoom: {PhotonNetwork.InRoom} | " +
            $"InLobby: {PhotonNetwork.InLobby}"
        );


        // Already connected to Master Server.
        if (PhotonNetwork.IsConnected &&
            PhotonNetwork.Server ==
            ServerConnection.MasterServer)
        {
            JoinLobby();

            return;
        }


        // Connected to GameServer from an old room.
        // Leave the room first.
        if (PhotonNetwork.InRoom)
        {
            Debug.Log(
                "Still inside a room. Leaving room first..."
            );

            PhotonNetwork.LeaveRoom();

            return;
        }


        // Not connected.
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log(
                "Connecting to Photon..."
            );

            PhotonNetwork.ConnectUsingSettings();
        }
    }


    // =====================================================
    // CONNECTED TO MASTER
    // =====================================================

    public override void OnConnectedToMaster()
    {
        Debug.Log(
            "CONNECTED TO MASTER SERVER"
        );

        JoinLobby();
    }


    // =====================================================
    // JOIN LOBBY
    // =====================================================

    private void JoinLobby()
    {
        if (PhotonNetwork.Server != ServerConnection.MasterServer)
        {
            Debug.LogWarning("Not on Master Server yet.");

            return;
        }


        if (PhotonNetwork.InLobby)
        {
            Debug.Log("Already in Lobby.");

            LoadLobbyScene();

            return;
        }


        Debug.Log("Joining Photon Lobby...");

        PhotonNetwork.JoinLobby();
    }


    // =====================================================
    // JOINED LOBBY
    // =====================================================

    public override void OnJoinedLobby()
    {
        Debug.Log(
            "JOINED PHOTON LOBBY SUCCESSFULLY"
        );

        Debug.Log(
            "Current Server: " +
            PhotonNetwork.Server
        );

        LoadLobbyScene();
    }


    // =====================================================
    // LEFT ROOM
    // =====================================================

    public override void OnLeftRoom()
    {
        Debug.Log(
            "Left old room. Returning to Master Server..."
        );

        // Photon automatically returns to Master Server.
        // Wait for OnConnectedToMaster().
    }


    // =====================================================
    // LOAD LOBBY
    // =====================================================

    private void LoadLobbyScene()
    {
        // IMPORTANT:
        // Never load Lobby while on GameServer.

        if (PhotonNetwork.Server !=
            ServerConnection.MasterServer)
        {
            Debug.LogWarning(
                "Cannot load Lobby yet. " +
                "Current Server: " +
                PhotonNetwork.Server
            );

            return;
        }


        if (_isLoadingLobby)
            return;


        if (SceneManager.GetActiveScene().name == lobbySceneName)
        {
            return;
        }


        _isLoadingLobby = true;


        Debug.Log(
            "Loading Lobby Scene..."
        );

        SceneManager.LoadScene(
            lobbySceneName
        );
    }


    // =====================================================
    // DISCONNECTED
    // =====================================================

    public override void OnDisconnected(
        DisconnectCause cause)
    {
        Debug.LogError(
            "Photon Disconnected: " +
            cause
        );
    }
}