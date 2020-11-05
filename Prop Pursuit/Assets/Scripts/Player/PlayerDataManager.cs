using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class PlayerDataItems // serialised data for save file
{
    public string username = "Guest";
    public int prop_totalgames = 0;
    public int prop_won = 0;
    public int prop_lost = 0;
    public int prop_streak = 0;
    public string prop_colour = "RGBA(1.000, 1.000, 1.000, 1.000)";
    public int hunter_totalgames = 0;
    public int hunter_won = 0;
    public int hunter_lost = 0;
    public int hunter_kills = 0;
    public string hunter_colour = "RGBA(1.000, 1.000, 1.000, 1.000)";
}

[System.Serializable]
public class PlayerData // array to hold save data
{
    // 1 set of data.
    public PlayerDataItems[] data = new PlayerDataItems[1]; 
}

public class PlayerDataManager : MonoBehaviour
{
    private static PlayerDataManager instance = null;
    public static PlayerDataManager Instance { get { return instance; } }

    public string playerName;

    public PlayerData playerData;

    public bool loadedData = false;
    public bool createdSave = false;

    public Color propColour;
    public Color hunterColour;

    private readonly string loadURL = "https://kunet.kingston.ac.uk/k1706289/proppursuit/controller/load_controller.php";
    private readonly string saveURL = "https://kunet.kingston.ac.uk/k1706289/proppursuit/controller/save_controller.php";

    private void Awake() // singleton setup
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        playerName = "Guest";
        // initialise as Guest
        playerData.data[0].username = playerName;
    }

    public void IncreaseGamesPlayed(bool prop)
    {
        if (prop)
            playerData.data[0].prop_totalgames++;
        else
            playerData.data[0].hunter_totalgames++;
    }

    public void IncreaseHunterKills()
    {
        playerData.data[0].hunter_kills++;
    }

    public void IncreaseGamesWon(bool prop)
    {
        if (prop)
            playerData.data[0].prop_won++;
        else
            playerData.data[0].hunter_won++;
    }

    public void IncreaseGamesLost(bool prop)
    {
        if (prop)
            playerData.data[0].prop_lost++;
        else
            playerData.data[0].hunter_lost++;
    }

    public void DecreaseGamesLost(bool prop)
    {
        if (prop)
            playerData.data[0].prop_lost--;
        else
            playerData.data[0].hunter_lost--;
    }

    public void SaveColour(bool prop, string newColour)
    {
        if (prop)
            playerData.data[0].prop_colour = newColour;
        else
            playerData.data[0].hunter_colour = newColour;

        RequestSave();
    }

    public void RequestSave()
    {
        if (playerData.data[0].username == "Guest" || playerData.data[0].username == "")
            playerData.data[0].username = playerName;

        StartCoroutine(Save());
    }

    IEnumerator Save()
    {
        WWWForm form = new WWWForm();

        playerData.data[0].ToString();
        string jsonSave = JsonUtility.ToJson(playerData.data[0]);

        form.AddField("savedata", jsonSave); // send JSON to server PHP

        UnityWebRequest webrequest = UnityWebRequest.Post(saveURL, form);

        yield return webrequest.SendWebRequest();

        if (webrequest.isNetworkError || webrequest.isHttpError)
        {
            Debug.Log(webrequest.error);
        }
        else
        {
            if (createdSave)
            {
                createdSave = false;
                RequestLoad(); // load data for newly created save data
            }

            Debug.Log("saved");
        }
    }

    public void RequestLoad()
    {
        loadedData = false;
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", playerName);

        UnityWebRequest webrequest = UnityWebRequest.Post(loadURL, form);

        yield return webrequest.SendWebRequest();
        if (webrequest.isNetworkError || webrequest.isHttpError)
        {
            Debug.Log(webrequest.error);
        }
        else
        {
            if (webrequest.downloadHandler.text != "failed") // found user
            {
                playerData = JsonUtility.FromJson<PlayerData>("{\"data\":" + webrequest.downloadHandler.text + "}");
                loadedData = true; // for profile page to load stats before displaying
                propColour = DecodeColour(playerData.data[0].prop_colour);
                hunterColour = DecodeColour(playerData.data[0].hunter_colour);
            }
            else
            {
                // no load data found. create save data
                createdSave = true;
                RequestSave();
            }
        }
    }

    public Color DecodeColour(string decodeColour) // convert the string to a colour value 
    {
        decodeColour = decodeColour.Replace("RGBA(", ""); // remove the wrapping
        decodeColour = decodeColour.Replace(")", "");

        string[] colValues = decodeColour.Split(',');
        return new Color(float.Parse(colValues[0]), float.Parse(colValues[1]), float.Parse(colValues[2]), float.Parse(colValues[3]));
    }
}
