using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerListing : MonoBehaviour
{
    public TextMeshProUGUI playerName;

    public PhotonPlayer PhotonPlayer { get; private set; }

    public void ApplyPhotonPlayer(PhotonPlayer photonPlayer)
    {
        PhotonPlayer = photonPlayer;
        playerName.text = photonPlayer.NickName;
    }
}
