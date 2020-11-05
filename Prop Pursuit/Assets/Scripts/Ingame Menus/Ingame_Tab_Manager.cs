using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingame_Tab_Manager : MonoBehaviour
{
    public Transform playerListingPage;
    public GameObject playerListingPrefab;
    private List<PlayerGameListing> playersDisplayed = new List<PlayerGameListing>();

    public static Ingame_Tab_Manager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private IEnumerator Start()
    {
        // clear old listings and reset list
        foreach (PlayerGameListing player in playersDisplayed)
        {
            Destroy(player.gameObject);
        }
        playersDisplayed.Clear();

        yield return new WaitUntil(() => GameManager.Instance.GameStarted == true);

        // add players to list
        PhotonPlayer[] photonPlayers = PhotonNetwork.playerList; // get list of all players in the room
        for (int i = 0; i < photonPlayers.Length; i++)
        {
            PlayerJoinedRoom(photonPlayers[i]);
        }
    }

    private void OnPhotonPlayerDisconnected(PhotonPlayer photonPlayer) //Photon callback player leaves room
    {
        PlayerLeftRoom(photonPlayer);
    }

    private void PlayerJoinedRoom(PhotonPlayer photonPlayer)
    {
        if (photonPlayer == null)
            return;

        // create new player to list
        GameObject newPlayer = Instantiate(playerListingPrefab, playerListingPage, false);
        PlayerGameListing playerGameListing = newPlayer.GetComponent<PlayerGameListing>();
        playerGameListing.ApplyPhotonPlayer(photonPlayer); // apply name from PlayerNetwork

        if (photonPlayer.ID == GameManager.Instance.RandomHunterSelect)
            playerGameListing.SetStatus("Hunter");
        else
            playerGameListing.SetStatus("Alive");

        playersDisplayed.Add(playerGameListing);
    }

    private void PlayerLeftRoom(PhotonPlayer photonPlayer)
    {
        int index = playersDisplayed.FindIndex(x => x.PhotonPlayer == photonPlayer);
        // player found
        if (index != -1)
        {
            playersDisplayed[index].SetStatus("Left Game");
        }
    }

    public void PlayerDied(int photonPlayer)
    {
        int index = playersDisplayed.FindIndex(x => x.PhotonPlayer.ID == photonPlayer);
        if (index != -1)
        {
            playersDisplayed[index].SetStatus("Dead");
        }
    }
}
