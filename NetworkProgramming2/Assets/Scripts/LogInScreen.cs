using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LogInScreen : MonoBehaviour
{
    public SignUpScreen signUpScreen;

    public TMP_InputField username;
    public TMP_InputField password;
    public TextMeshProUGUI error;
    public ClientBehaviour client;

    private void Awake()
    {
        error.text = string.Empty;
    }

    public void Login()
    {
        client.Login(username.text, password.text);
        //StartCoroutine(LoginEnumerator());
    }


    public void GoToSignUpScreen()
    {
        signUpScreen.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Logout()
    {

    }
}
