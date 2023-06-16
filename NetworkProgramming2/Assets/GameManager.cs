using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    _instance = new GameObject("GameManager").AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    private static GameManager _instance;

    public ClientBehaviour client;
    ClientBehaviour currentClient;

    public GameObject connectButtons;
    public SoundManager soundManager;

    public void ConnectToServer()
    {
        currentClient = Instantiate(client);
    }

    public void DisconnectFromServer()
    {
        Destroy(currentClient.gameObject);
        connectButtons.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
