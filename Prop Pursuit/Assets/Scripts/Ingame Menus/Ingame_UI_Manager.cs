using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Ingame_UI_Manager : MonoBehaviour
{
    public static Ingame_UI_Manager instance;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI playerText;
    public TextMeshProUGUI gameoverText;
    public TextMeshProUGUI leaveGameText;

    private void Awake()
    {
        instance = this;
    }
    public void SetHealthText(int health)
    {
        healthText.text = "Health: " + health.ToString();
    }

    public void SetHunterTimerText(float time)
    {
        timerText.text = "Time until Hunter spawns: " + time.ToString("0");
    }

    public void SetSurvivalTimerText(float time)
    {
        timerText.text = "Time until Hunter Leaves: " + time.ToString("0");
    }

    public void SetPlayersAliveText(int alive)
    {
        playerText.text = "Players Alive: " + alive.ToString();
    }

    public void SetGameOverText(string text)
    {
        gameoverText.text = text;
        leaveGameText.text = "press escape to leave game";
    }
}
