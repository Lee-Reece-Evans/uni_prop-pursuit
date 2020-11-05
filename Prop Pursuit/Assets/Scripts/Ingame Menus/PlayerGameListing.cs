using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerGameListing : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerStatus;

    public PhotonPlayer PhotonPlayer { get; private set; }

    private void Start()
    {
        playerStatus.color = Color.green;
        playerName.color = Color.green;
    }

    public void ApplyPhotonPlayer(PhotonPlayer photonPlayer)
    {
        PhotonPlayer = photonPlayer;
        playerName.text = photonPlayer.NickName;
    }

    public void SetStatus(string status)
    {
        playerStatus.text = status;
    }
}
