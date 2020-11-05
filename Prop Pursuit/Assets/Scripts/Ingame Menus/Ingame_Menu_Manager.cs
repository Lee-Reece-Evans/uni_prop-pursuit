using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingame_Menu_Manager : MonoBehaviour
{
    public GameObject gameMenu;
    public GameObject settingsMenu;
    public GameObject tabMenu;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            tabMenu.SetActive(true);
        if (Input.GetKeyUp(KeyCode.Tab))
            tabMenu.SetActive(false);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsMenu.activeSelf)
                CloseSettingsMenu();
            else if (gameMenu.activeSelf)
                ResumeGame();
            else
            {
                ShowMenu();
                GameManager.Instance.acceptInputs = false;
            }
        }
    }

    private void ShowMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HideMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowMenu()
    {
        gameMenu.SetActive(true);
        ShowMouse();
    }

    public void ResumeGame()
    {
        gameMenu.SetActive(false);
        HideMouse();
        GameManager.Instance.acceptInputs = true;
    }

    public void SettingsMenu()
    {
        gameMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void CloseSettingsMenu()
    {
        settingsMenu.SetActive(false);
        gameMenu.SetActive(true);
    }

    public void LeaveGame()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(1);
    }
}
