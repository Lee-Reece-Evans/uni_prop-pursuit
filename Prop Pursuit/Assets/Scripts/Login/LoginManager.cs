using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    private readonly string loginURL = "https://kunet.kingston.ac.uk/k1706289/proppursuit/controller/login_controller.php";
    private readonly string createAccountURL = "https://kunet.kingston.ac.uk/k1706289/proppursuit/controller/create_account_controller.php";

    public InputField loginUsername;
    public InputField loginPassword;
    public InputField createUsername;
    public InputField createPassword;
    public Text loginError;
    public Text createError;

    public void RequestLogin()
    {
        string user = loginUsername.text;
        string pass = loginPassword.text;

        StartCoroutine(Login(user, pass));
    }

    IEnumerator Login(string user, string pass)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", user);
        form.AddField("password", pass);

        UnityWebRequest webrequest = UnityWebRequest.Post(loginURL, form);

        yield return webrequest.SendWebRequest();

        if (webrequest.isNetworkError || webrequest.isHttpError)
        {
            Debug.Log(webrequest.error);
        }
        else
        {
            if (webrequest.downloadHandler.text == user) // found user
            {
                PlayerDataManager.Instance.playerName = webrequest.downloadHandler.text; // set the users name in the data manager
                // load player stats on main menu
                SceneManager.LoadScene(1); // load main menu
            }
            else
            {
                loginError.text = "ERROR: " + webrequest.downloadHandler.text;
            }
        }
    }

    public void RequestCreateAccount()
    {
        string user = createUsername.text;
        string pass = createPassword.text;

        // set restrictions of username and password acceptance
        if (user.Length < 5)
            createError.text = "ERROR: Username must be between 5 and 15 characters";
        else if (user.Contains(" "))
            createError.text = "ERROR: Username must not contain spaces";
        else if (pass.Length < 5)
            createError.text = "ERROR: Password must be between 5 and 8 Characters Long";
        else if (pass.Contains(" "))
            createError.text = "ERROR: Password must not contain spaces";
        else
            StartCoroutine(CreateAccount(user, pass));
    }

    IEnumerator CreateAccount(string user, string pass)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", user);
        form.AddField("password", pass);

        UnityWebRequest webrequest = UnityWebRequest.Post(createAccountURL, form); // send inputs to server url php file

        yield return webrequest.SendWebRequest();

        if (webrequest.isNetworkError || webrequest.isHttpError) // check if there was an issue sending
        {
            Debug.Log(webrequest.error); // display the error
        }
        else
        {
            if (webrequest.downloadHandler.text == "success") // account created, now login automatically
            {
                Debug.Log("made account");
                StartCoroutine(Login(user, pass)); // loging the new account
            }
            else
            {
                createError.text = "ERROR: " + webrequest.downloadHandler.text; // display a message with an error
            }
        }
    }
}
