using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ProfileManager : MonoBehaviour
{
    public TextMeshProUGUI prop_totalgames;
    public TextMeshProUGUI prop_wongames;
    public TextMeshProUGUI prop_lostgames;
    public TextMeshProUGUI prop_ratio;
    public TextMeshProUGUI prop_winstreak;
    public TextMeshProUGUI hunter_totalgames;
    public TextMeshProUGUI hunter_wongames;
    public TextMeshProUGUI hunter_lostgames;
    public TextMeshProUGUI hunter_ratio;
    public TextMeshProUGUI hunter_kills;
    public Image propColourPreview;
    public Image hunterColourPreview;

    public Renderer hunterMat;
    public Renderer propMat;

    private Color propColour;
    private Color hunterColour;

    private IEnumerator Start()
    {
        PlayerDataManager.Instance.RequestLoad();

        yield return new WaitUntil(() => PlayerDataManager.Instance.loadedData == true);

        propColour = PlayerDataManager.Instance.propColour;
        hunterColour = PlayerDataManager.Instance.hunterColour;
        propColourPreview.color = propColour;
        hunterColourPreview.color = hunterColour;
        SetMenuCharacterColours();
        SetPropStats();
        SetHunterStats();
    }

    private void SetPropStats()
    {
        prop_totalgames.text = "Games Played: " + PlayerDataManager.Instance.playerData.data[0].prop_totalgames;
        prop_wongames.text = "Games Won: " + PlayerDataManager.Instance.playerData.data[0].prop_won;
        prop_lostgames.text = "Games Lost: " + PlayerDataManager.Instance.playerData.data[0].prop_lost;

        float propRatio;
        if (PlayerDataManager.Instance.playerData.data[0].prop_lost != 0)
            propRatio = (float)PlayerDataManager.Instance.playerData.data[0].prop_won / (float)PlayerDataManager.Instance.playerData.data[0].prop_lost;
        else
            propRatio = PlayerDataManager.Instance.playerData.data[0].prop_won;

        prop_ratio.text = "W/L Ratio: " + propRatio;

        prop_winstreak.text = "Longest Streak: " + PlayerDataManager.Instance.playerData.data[0].prop_streak;
    }

    private void SetHunterStats()
    {
        hunter_totalgames.text = "Games Played: " + PlayerDataManager.Instance.playerData.data[0].hunter_totalgames;
        hunter_wongames.text = "Games Won: " + PlayerDataManager.Instance.playerData.data[0].hunter_won;
        hunter_lostgames.text = "Games Lost: " + PlayerDataManager.Instance.playerData.data[0].hunter_lost;
        float hunterRatio;
        if (PlayerDataManager.Instance.playerData.data[0].hunter_lost != 0)
            hunterRatio = (float)PlayerDataManager.Instance.playerData.data[0].hunter_won / (float)PlayerDataManager.Instance.playerData.data[0].hunter_lost;
        else
            hunterRatio = PlayerDataManager.Instance.playerData.data[0].hunter_won;

        hunter_ratio.text = "W/L Ratio: " + hunterRatio;

        hunter_kills.text = "Total Kills: " + PlayerDataManager.Instance.playerData.data[0].hunter_kills;
    }

    public void ChooseRandomColour(bool prop)
    {
        Color newColour = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        if (prop)
        {
            propColourPreview.color = newColour;
            propColour = newColour;
        }
        else
        {
            hunterColourPreview.color = newColour;
            hunterColour = newColour;
        }
    }

    private void SetMenuCharacterColours()
    {
        propMat.material.color = propColour;
        hunterMat.material.color = hunterColour;
    }

    public void ApplyColour(bool prop)
    {
        SetMenuCharacterColours();
        if (prop)
        {
            PlayerDataManager.Instance.propColour = propColour;
            PlayerDataManager.Instance.SaveColour(prop, propColour.ToString());
        }
        else
        {
            PlayerDataManager.Instance.hunterColour = hunterColour;
            PlayerDataManager.Instance.SaveColour(prop, hunterColour.ToString());
        }
    }
}
