using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SignUpScreen : MonoBehaviour
{
    public LogInScreen loginScreen;

    public TMP_InputField emailAddress;
    public TMP_InputField username;
    public TMP_InputField password;
    public TMP_InputField confirmPassword;
    public TextMeshProUGUI error;

    private void Awake()
    {
        error.text = string.Empty;
    }

    public void SignUp()
    {
        StartCoroutine(SignUpEnumerator());
    }

    public void BackToLoginScreen()
    {
        loginScreen.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    IEnumerator SignUpEnumerator()
    {
        WWWForm form = new WWWForm();
        form.AddField("email", emailAddress.text);
        form.AddField("username", username.text);
        form.AddField("password", password.text);
        form.AddField("confirmPassword", confirmPassword.text);

        using (UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~maxym.ebeling/Register.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                error.text = www.error;
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log(responseText);
                if (responseText.StartsWith("\"Success\""))
                {
                    Debug.Log("Account Registered");
                    loginScreen.error.text = "Registration Complete";
                    BackToLoginScreen();
                }
                else
                {
                    error.text = responseText;
                }
            }
        }
    }
}
