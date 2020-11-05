using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public GameObject[] menus;
    public GameObject menuButtons;
    public GameObject roomButtons;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI RoomName; // used in player menu

    private void Start()
    {
        if (playerName != null)
            playerName.text = "Welcome " + PlayerDataManager.Instance.playerName + "!";
    }

    public void DisplayMenu(string menuName)
    {
        if (menuName == "Player_Room")
        {
            if (!roomButtons.activeSelf)
            {
                menuButtons.SetActive(false);
                roomButtons.SetActive(true);
            }
        }
        else if (menuName == "Room_List")
        {
            if (!menuButtons.activeSelf)
            {
                roomButtons.SetActive(false);
                menuButtons.SetActive(true);
            }
        }

        foreach (GameObject menu in menus)
        {
            if (menu.name != menuName && menu.activeSelf)
                menu.SetActive(false);
            else if (menu.name == menuName && !menu.activeSelf)
                menu.SetActive(true);
        }
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
}
