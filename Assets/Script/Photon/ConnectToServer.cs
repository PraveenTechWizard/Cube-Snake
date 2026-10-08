using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    // =====================================================
    // START
    // =====================================================

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
            "[PHOTON] Connected: " +
            PhotonNetwork.IsConnected +
            " | Server: " +
            PhotonNetwork.Server +
            " | InRoom: " +
            PhotonNetwork.InRoom);


        // Already connected to Master Server.
        if (PhotonNetwork.IsConnected &&
            PhotonNetwork.Server ==
            ServerConnection.MasterServer)
        {
            Debug.Log("[PHOTON] Already connected to Master Server.");

            return;
        }


        // Still inside an old room.
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("[PHOTON] Still inside old room. Leaving...");

            PhotonNetwork.LeaveRoom();

            return;
        }


        // Not connected.
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("[PHOTON] Connecting to Photon...");

            PhotonNetwork.ConnectUsingSettings();

            return;
        }
    }


    // =====================================================
    // CONNECTED TO MASTER
    // =====================================================

    public override void OnConnectedToMaster()
    {
        Debug.Log("[PHOTON] CONNECTED TO MASTER SERVER");

        Debug.Log("[PHOTON] Ready for room operations.");
    }


    // =====================================================
    // LEFT ROOM
    // =====================================================

    public override void OnLeftRoom()
    {
        Debug.Log("[PHOTON] Left old room.");
    }


    // =====================================================
    // DISCONNECTED
    // =====================================================

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("[PHOTON] Disconnected: " + cause);
    }
}