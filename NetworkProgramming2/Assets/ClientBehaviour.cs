using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

public delegate void ClientMessageHandler(ClientBehaviour client, BaseMessage header);

public class ClientBehaviour : MonoBehaviour
{
    static Dictionary<NetworkMessageType, ClientMessageHandler> networkMessageHandlers = new Dictionary<NetworkMessageType, ClientMessageHandler> {
            { NetworkMessageType.REGISTER_RESPONSE,         HandleServerRegisterResponseMessage },
            { NetworkMessageType.HANDSHAKE_RESPONSE,        HandleServerHandshakeResponseMessage },
            { NetworkMessageType.NETWORK_SPAWN,             HandleNetworkSpawnMessage },               // uint networkId, uint objectType
            { NetworkMessageType.PLAYER_READY_RESPONSE,     HandlePlayerReadyResponseMessage },
            { NetworkMessageType.START_GAME,                HandleStartGameMessage },
            { NetworkMessageType.RPC_SERVER,                HandleServerRPCMessage },           // uint networkId, vector3 position, vector3 rotation
            { NetworkMessageType.DESTROY,                   HandleDestroyMessage },
            { NetworkMessageType.END_GAME,                  HandleEndGame },
            { NetworkMessageType.PING,                      HandlePing }
        };

    public NetworkDriver m_Driver;
    public NetworkPipeline m_Pipeline;
    public NetworkConnection m_Connection;
    public bool Done;
    public NetworkedManager networkManager;


    public static string serverIP;
    public string clientName = "";
    uint playerID;

    bool connected = false;
    float startTime = 0;

    public LogInScreen logInScreen;
    public GameObject readyScreen;
    public GameObject readyButton;
    public GameObject unreadyButton;
    public GameObject disconnectButton;

    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI winLoseRate;
    public TextMeshProUGUI totalGamesPlayed;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;

        IPAddress serverAddress = IPAddress.Parse("217.105.54.134");                                                //????
        NativeArray<byte> nativeArrayAddress;                                                                       //????
        // Convert that into a NativeArray of byte data                                                             //????
        nativeArrayAddress = new NativeArray<byte>(serverAddress.GetAddressBytes().Length, Allocator.Temp);         //????
        nativeArrayAddress.CopyFrom(serverAddress.GetAddressBytes());                                               //????

        // Create connection to server IP
        m_Driver = NetworkDriver.Create();
        m_Pipeline = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.Parse(serverIP, 9000, NetworkFamily.Ipv4);
        endpoint.SetRawAddressBytes(nativeArrayAddress);         // Use SetRawAddressBytes                          //????
        endpoint.Port = 9000;
        m_Connection = m_Driver.Connect(endpoint);
        logInScreen.client = this;
    }

    // No collections list this time...
    void OnApplicationQuit()
    {
        // Disconnecting on application exit currently (to keep it simple)
        if (m_Connection.IsCreated)
        {
            m_Connection.Disconnect(m_Driver);
            m_Connection = default(NetworkConnection);
        }
    }

    void OnDestroy()
    {
        m_Driver.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!connected && Time.time - startTime > 5f)
        {
            SceneManager.LoadScene(0);
        }

        if (!m_Connection.IsCreated)
        {
            if (!Done)
                Debug.Log("Something went wrong during connect");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                connected = true;
                Debug.Log("We are now connected to the server");

                // TODO: Create handshake message
                //var header = new HandshakeMessage();
                //SendPackedMessage(header);
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                Done = true;

                // First UInt is always message type (this is our own first design choice)
                NetworkMessageType msgType = (NetworkMessageType)stream.ReadUShort();

                // TODO: Create message instance, and parse data...
                BaseMessage header = (BaseMessage)System.Activator.CreateInstance(NetworkMessageInfo.TypeMap[msgType]);
                header.DeserializeObject(ref stream);

                if (networkMessageHandlers.ContainsKey(msgType))
                {
                    networkMessageHandlers[msgType].Invoke(this, header);
                }
                else
                {
                    Debug.LogWarning($"Unsupported message type received: {msgType}", this);
                }
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }
        }
    }

    public void SendPackedMessage(BaseMessage header)
    {
        DataStreamWriter writer;
        int result = m_Driver.BeginSend(m_Pipeline, m_Connection, out writer);

        // non-0 is an error code
        if (result == 0)
        {
            header.SerializeObject(ref writer);
            m_Driver.EndSend(writer);
        }
        else
        {
            Debug.LogError($"Could not wrote message to driver: {result}", this);
        }
    }

    public void CallRPCOnServer(string function, NetworkedBehaviour target, params object[] data)
    {
        RPCClientMessage rpc = new RPCClientMessage
        {
            target = target,
            methodName = function,
            data = data
        };
        SendPackedMessage(rpc);
    }

    public void Login(string _username, string _password)
    {
        var header = new HandshakeMessage();
        header.username = _username;
        header.password = _password;
        SendPackedMessage(header);
    }

    public void Register(string _emailAddress, string _username, string _password, string _confirmPassword)
    {
        var header = new RegisterMessage();
        header.emailAddress = _emailAddress;
        header.username = _username;
        header.password = _password;
        header.confirmPassword = _confirmPassword;
        SendPackedMessage(header);
    }

    static void HandleServerRegisterResponseMessage(ClientBehaviour client, BaseMessage header)
    {
        RegisterResponseMessage response = header as RegisterResponseMessage;
        client.logInScreen.signUpScreen.BackToLoginScreen();
    }

    static void HandleServerHandshakeResponseMessage(ClientBehaviour client, BaseMessage header)
    {
        HandshakeResponseMessage response = header as HandshakeResponseMessage;
        if (response.connected == 1)
        {
            client.clientName = response.responseText;
            client.readyScreen.SetActive(true);
            client.logInScreen.gameObject.SetActive(false);
        }
        else
        {
            client.logInScreen.error.text = response.responseText;
        }
        //NetworkedBehaviour obj;
        //if (client.networkManager.SpawnNewObjectWithId(NetworkSpawnObject.PLAYER, response.networkID, out obj))
        //{
        //    NetworkedPlayer player = obj.GetComponent<NetworkedPlayer>();
        //    player.isLocal = true;
        //    player.isServer = false;
        //}
        //else
        //{
        //    Debug.LogError("Could not spawn player!");
        //}
    }

    static void HandleNetworkSpawnMessage(ClientBehaviour client, BaseMessage header)
    {
        SpawnMessage spawnMsg = header as SpawnMessage;

        NetworkedBehaviour obj;
        if (client.networkManager.SpawnNewObjectWithId(spawnMsg.objectType, spawnMsg.networkID, out obj))
        {
            obj.transform.position = new Vector2(spawnMsg.xPos, spawnMsg.yPos);
        }
        else
        {
            Debug.LogError($"Could not spawn {spawnMsg.objectType} for id {spawnMsg.networkID}!");
        }
    }

    public void ReadyPlayer()
    {
        var header = new PlayerReadyMessage();
        SendPackedMessage(header);
    }

    public async void Disconnect()
    {
        networkManager.ClearDict();

        var header = new PlayerQuitMessage();
        header.networkID = playerID;
        SendPackedMessage(header);

        await Task.Delay(100);
        GameManager.Instance.DisconnectFromServer();
    }

    static void HandlePlayerReadyResponseMessage(ClientBehaviour client, BaseMessage header)
    {
        PlayerReadyResponseMessage responseMsg = header as PlayerReadyResponseMessage;

        //ready player instance
        //NetworkedBehaviour obj;
        //client.networkManager.GetObject(responseMsg.networkID, out obj);
        //obj.GetComponent<NetworkedPlayer>().ready = true;
    }

    static void HandleStartGameMessage(ClientBehaviour client, BaseMessage header)
    {
        client.networkManager.ClearDict();

        StartGameMessage startMsg = header as StartGameMessage;
        //start game
        NetworkedBehaviour obj;
        if (client.networkManager.SpawnNewObjectWithId(startMsg.objectType, startMsg.networkID, out obj))
        {
            obj.GetComponent<NetworkedPlayer>().isLocal = true;
            obj.GetComponent<NetworkedPlayer>().ready = true;
            obj.transform.position = new Vector2(startMsg.xPos, startMsg.yPos);
            client.playerID = startMsg.networkID;
        }
        else
        {
            Debug.LogError($"Could not spawn {startMsg.objectType} for id {startMsg.networkID}!");
        }
        client.winnerText.gameObject.SetActive(false);
        client.winLoseRate.gameObject.SetActive(false);
        client.totalGamesPlayed.gameObject.SetActive(false);
        client.readyButton.SetActive(false);
        client.unreadyButton.SetActive(false);
        client.disconnectButton.SetActive(false);
    }

    static void HandleServerRPCMessage(ClientBehaviour client, BaseMessage header)
    {
        RPCServerMessage msg = header as RPCServerMessage;

        // try to call the function
        try
        {
            msg.mInfo.Invoke(msg.target, msg.data);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
    }

    static void HandleDestroyMessage(ClientBehaviour client, BaseMessage header)
    {
        DestroyMessage destroyMessage = header as DestroyMessage;
        Debug.Log(destroyMessage.networkID);
        client.networkManager.DestroyWithId(destroyMessage.networkID);
    }

    static void HandleEndGame(ClientBehaviour client, BaseMessage header)
    {
        EndGameMessage endGameMessage = header as EndGameMessage;
        //display winner
        client.winnerText.gameObject.SetActive(true);
        client.winnerText.text = endGameMessage.winnerName + " has won!";
        client.winLoseRate.gameObject.SetActive(true);
        client.winLoseRate.text = "W/L " + endGameMessage.wins + "/" + endGameMessage.loses;
        client.totalGamesPlayed.gameObject.SetActive(true);
        client.totalGamesPlayed.text = "Total games played: " + endGameMessage.totalGames.ToString();

        client.readyButton.SetActive(true);
        client.unreadyButton.SetActive(false);
        client.disconnectButton.SetActive(true);        
        //NetworkedBehaviour obj;
        //client.networkManager.GetObject(client.playerID, out obj);
        //obj.GetComponent<NetworkedPlayer>().ready = false;
    }

    static void HandlePing(ClientBehaviour client, BaseMessage header)
    {
        PongMessage pongMsg = new PongMessage();
        client.SendPackedMessage(pongMsg);
    }
}
