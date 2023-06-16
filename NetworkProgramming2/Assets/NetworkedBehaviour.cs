using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedBehaviour : MonoBehaviour
{
    public ClientBehaviour client;
    public ServerBehaviour server;
    public bool isLocal = false;
    public bool isServer = false;

    public uint networkID = 0;

    private void Start()
    {
        if (isServer)
        {
            server = FindObjectOfType<ServerBehaviour>();
        }
        else
        {
            client = FindObjectOfType<ClientBehaviour>();
        }
    }
}
