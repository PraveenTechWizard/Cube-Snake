using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerNetworkSetup : MonoBehaviourPun
{
    [Header("Player Name")]
    [SerializeField]
    private TextMeshPro playerNameText;


    private void Start()
    {
        SetupPlayer();
    }


    // =====================================================
    // SETUP
    // =====================================================

    private void SetupPlayer()
    {
        if (photonView == null)
        {
            Debug.LogError(
                "PhotonView is missing!"
            );

            return;
        }


        // =================================================
        // GET OWNER NAME
        // =================================================

        string playerName =
            photonView.Owner.NickName;


        // =================================================
        // DISPLAY NAME
        // =================================================

        if (playerNameText != null)
        {
            playerNameText.text =
                playerName;
        }


        Debug.Log(
            "Player spawned: " +
            playerName +
            " | Is Mine: " +
            photonView.IsMine
        );


        // =================================================
        // LOCAL PLAYER ONLY
        // =================================================

        if (photonView.IsMine)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupRemotePlayer();
        }
    }


    // =====================================================
    // LOCAL PLAYER
    // =====================================================

    private void SetupLocalPlayer()
    {
        Debug.Log(
            "This is MY player: " +
            photonView.Owner.NickName
        );
    }


    // =====================================================
    // REMOTE PLAYER
    // =====================================================

    private void SetupRemotePlayer()
    {
        Debug.Log(
            "This is OTHER player: " +
            photonView.Owner.NickName
        );
    }
}