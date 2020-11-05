using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoomListing : MonoBehaviour
{
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI currentPlayers;
    public string roomPassword;
    public TMP_InputField passwordInput;
    public GameObject passwordInputGO;

    public bool updatedRoom;

    private void Start()
    {
        RoomInfo[] rooms = PhotonNetwork.GetRoomList(); // get a list of all rooms info

        foreach (RoomInfo room in rooms) // loop through rooms to find selected room
        {
            if (room.Name == roomName.text && room.CustomProperties["Password"] != null) // check if chosen room has a password set
            {
                roomPassword = room.CustomProperties["Password"].ToString(); // store the room password
            }
        }
        Debug.Log("room password = " + roomPassword);

        if (!string.IsNullOrEmpty(roomPassword)) // if a password has been set, show the input field GO
        {
            passwordInputGO.SetActive(true);
        }
    }

    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(roomPassword)) // no password set for room
        {
            TryToJoin();
        }
        else
        {
            if (passwordInput.text == roomPassword) // check input password matches the room password
            {
                TryToJoin();
            }
            else
            {
                passwordInput.text = "!!WRONG PASSWORD!!";
            }
        }
    }

    private void TryToJoin()
    {
        if (PhotonNetwork.JoinRoom(roomName.text))
            Debug.Log("joined room");
        else
            Debug.Log("Failed to join room");
    }
}
