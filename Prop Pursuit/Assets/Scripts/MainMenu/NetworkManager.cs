using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    [Header("Room Listings")]
    public InputField roomName; // room name input
    public InputField roomPassword; // room name input
    public GameObject roomListingPrefab; // room list object
    public Transform roomListingPage; // where to dock room listings
    private RoomInfo[] roomsList; // collection of active rooms found
    private List<RoomListing> roomsDisplayed = new List<RoomListing>(); // rooms being displayed

    [Header("Player Listings")]
    public GameObject playerListingPrefab; // player list object
    public Transform playerListingPage; // where to dock player listing
    private List<PlayerListing> playersDisplayed = new List<PlayerListing>(); // players being displayed

    private TypedLobby lobbyName = new TypedLobby("new_lobby", LobbyType.Default);
    public MenuManager menuManager; // reference to menu manager

    void Start()
    {
        if (!PhotonNetwork.connected)
            PhotonNetwork.ConnectUsingSettings("v4.2");
    }

    public void CreateRoom() // function for player to create a new room
    {   // room options has a lobby custom propery so that the room custom property for password can be accessed from room info
        if (PhotonNetwork.CreateRoom(roomName.text, new RoomOptions() {
            MaxPlayers = 5, IsOpen = true, IsVisible = true, CustomRoomPropertiesForLobby = new string[1] { "Password" }, CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Password", roomPassword.text } } }, lobbyName))
        {
            Debug.Log("Room Created");
        }
        else
            Debug.Log("Failed to create room");
    }

    private void OnCreatedRoom() // photon callback once player has created a room
    {
        Debug.Log("Room is now open");
    }

    void OnConnectedToMaster() // photon callback once player connected to network
    {
        PhotonNetwork.automaticallySyncScene = true; // all players load level with master
        PhotonNetwork.playerName = PlayerDataManager.Instance.playerName;
        PhotonNetwork.JoinLobby(lobbyName);
    }

    void OnJoinedLobby() // photon callback once player has joined the lobby
    {
        Debug.Log("Joined Lobby");
        if (!PhotonNetwork.inRoom && !menuManager.menuButtons.activeSelf)
        {
            menuManager.DisplayMenu("Room_List");
        }
    }

    void OnReceivedRoomListUpdate() // photon callback to recieve room listing updates
    {
        roomsList = PhotonNetwork.GetRoomList();
        ShowRooms();
    }

    private void ShowRooms() // function to show active rooms
    {
        if (roomsList != null) // if there are room listings
        {
            foreach (RoomInfo room in roomsList)
            {
                int index = roomsDisplayed.FindIndex(x => x.roomName.text == room.Name); // check if room is found
                if (index == -1) // if no room found then create it
                {
                    if (room.IsVisible && room.PlayerCount < room.MaxPlayers)
                    {
                        GameObject newRoom = Instantiate(roomListingPrefab, roomListingPage, false); // create the roomlisting prefab GO
                        RoomListing roomListing = newRoom.GetComponent<RoomListing>(); // get the room listing script

                        roomListing.roomName.text = room.Name; // set the name for the roomlisting prefab to display
                        roomListing.currentPlayers.text = "Players: " + room.PlayerCount.ToString() + "/5"; // display players in the room
                        roomListing.updatedRoom = true; // make room initially active
                        roomsDisplayed.Add(roomListing); // add this room to list of active displayed rooms
                    }
                }
                if (index != -1) // update an exsisting room
                {
                    RoomListing roomListing = roomsDisplayed[index]; // get the currently found rooms roomlisting script
                    roomListing.currentPlayers.text = "Players: " + room.PlayerCount.ToString() + "/5"; // display players in the room
                    roomListing.updatedRoom = true; // room is still active
                }
            }
            // remove old rooms
            List<RoomListing> removeRoom = new List<RoomListing>();
            foreach (RoomListing room in roomsDisplayed)
            {
                if (!room.updatedRoom) // has not got an update, must not exsist anymore
                    removeRoom.Add(room); // add it to list of rooms to remove
                else
                    room.updatedRoom = false; // set it to false, will be set to true again if it still exsists in next function call.
            }
            // destroy all of the following
            foreach (RoomListing room in removeRoom)
            {
                GameObject deadRoom = room.gameObject;
                roomsDisplayed.Remove(room);
                Destroy(deadRoom);
            }
        }
    }

    private void OnMasterClientSwitched(PhotonPlayer newMasterClient) // Photon callback to all clients when master leaves room
    {
        LeaveRoom(); // all players leave when masterclient leaves
    }

    private void OnJoinedRoom() // photon callback when a player has joined a room
    {
        // set room name text
        menuManager.RoomName.text = "Room: " + PhotonNetwork.room.Name;

        //set start button availability
        if (!PhotonNetwork.isMasterClient)
        {
            menuManager.roomButtons.transform.Find("Start_Game").GetComponent<Button>().interactable = false; // don't allow non master clients to click start
        }

        //display menu
        menuManager.DisplayMenu("Player_Room");

        // clear old listings and reset list
        foreach (PlayerListing player in playersDisplayed)
        {
            Destroy(player.gameObject);
        }
        playersDisplayed.Clear();

        PhotonPlayer[] photonPlayers = PhotonNetwork.playerList; // get list of all players in the room (adding listings upon joining room)
        for (int i = 0; i < photonPlayers.Length; i++)
        {
            PlayerJoinedRoom(photonPlayers[i]);
        }
    }

    private void OnPhotonPlayerConnected(PhotonPlayer photonPlayer) // photon call when player joins room (used for updating player listings for players already in the room)
    {
        PlayerJoinedRoom(photonPlayer);
    }

    private void OnPhotonPlayerDisconnected(PhotonPlayer photonPlayer) //Photon callback player leaves room destroy their listing
    {
        PlayerLeftRoom(photonPlayer);
    }

    private void PlayerJoinedRoom(PhotonPlayer photonPlayer)
    {
        if (photonPlayer == null)
            return;

        // create new player to list
        GameObject newPlayer = Instantiate(playerListingPrefab, playerListingPage, false);
        PlayerListing playerListing = newPlayer.GetComponent<PlayerListing>();
        playerListing.ApplyPhotonPlayer(photonPlayer); // apply name from PlayerNetwork

        playersDisplayed.Add(playerListing); // add playerlisting to list of currently displayed players
    }

    private void PlayerLeftRoom(PhotonPlayer photonPlayer)
    {
        int index = playersDisplayed.FindIndex(x => x.PhotonPlayer == photonPlayer); // see if player exsists in the list.
        // player found
        if (index != -1)
        {
            Destroy(playersDisplayed[index].gameObject); // destroy the playerlisting object
            playersDisplayed.RemoveAt(index); // renove the entry from the list
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void StartGame()
    {
        if (PhotonNetwork.room.PlayerCount > 1) // only start map if there are atleast 2 players
        {
            PhotonNetwork.room.IsOpen = false;
            PhotonNetwork.room.IsVisible = false;
            PhotonNetwork.LoadLevel(2);
        }
    }
}
