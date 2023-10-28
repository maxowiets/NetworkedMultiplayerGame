using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Networking;

public delegate void ServerMessageHandler(ServerBehaviour server, NetworkConnection con, BaseMessage header);

public class ServerBehaviour : MonoBehaviour
{
    static Dictionary<NetworkMessageType, ServerMessageHandler> networkMessageHandlers = new Dictionary<NetworkMessageType, ServerMessageHandler> {
                { NetworkMessageType.REGISTER,      HandleClientRegister },
                { NetworkMessageType.HANDSHAKE,     HandleClientHandshake },
                { NetworkMessageType.PLAYER_READY,  HandlePlayerReady },
                { NetworkMessageType.RPC_CLIENT,    HandleClientRPCMessage },
                { NetworkMessageType.PLAYER_QUIT,   HandleClientExit },
                { NetworkMessageType.PONG,          HandleClientPong }
            };
    public class PingPong
    {
        public float lastSendTime = 0;
        public int status = -1;
        public int playerID;
    }
    public NetworkDriver m_Driver;
    public NetworkPipeline m_Pipeline;
    private NativeList<NetworkConnection> m_Connections;
    private Dictionary<NetworkConnection, NetworkedPlayer> playerInstances = new Dictionary<NetworkConnection, NetworkedPlayer>();
    private Dictionary<NetworkConnection, PingPong> pongDict = new Dictionary<NetworkConnection, PingPong>();

    private int player1ID = -1;
    private string player1Name = "";
    private int player1IDcon = -1;
    private int player2ID = -1;
    private string player2Name = "";
    private int player2IDcon = -1;
    private bool readyPlayer1;
    private bool readyPlayer2;
    private bool gameStarted;
    public Vector2 spawnLoc1;
    public Vector2 spawnLoc2;
    public uint currentPlayerTurn;
    public string sessionID = "";
    public int serverID = 1;
    public string serverPassword = "09ahw8h098hasiuedh";

    int playerCount;

    public NetworkedManager networkManager;

    void Start()
    {
        StartCoroutine(GetSessionID());
    }

    // Write this immediately after creating the above Start calls, so you don't forget
    //  Or else you well get lingering thread sockets, and will have trouble starting new ones!
    void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    void Update()
    {
        // This is a jobified system, so we need to tell it to handle all its outstanding tasks first
        m_Driver.ScheduleUpdate().Complete();

        // Clean up connections, remove stale ones
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                // This little trick means we can alter the contents of the list without breaking/skipping instances
                --i;
            }
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");
        }

        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
                continue;

            // Loop through available events
            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    // First UInt is always message type (this is our own first design choice)
                    NetworkMessageType msgType = (NetworkMessageType)stream.ReadUShort();

                    // Create instance and deserialize
                    BaseMessage header = (BaseMessage)System.Activator.CreateInstance(NetworkMessageInfo.TypeMap[msgType]);
                    header.DeserializeObject(ref stream);

                    if (networkMessageHandlers.ContainsKey(msgType))
                    {
                        try
                        {
                            networkMessageHandlers[msgType].Invoke(this, m_Connections[i], header);
                        }
                        catch
                        {
                            Debug.LogError($"Badly formatted message received: {msgType}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Unsupported message type received: {msgType}", this);
                    }
                }
            }
        }

        // Ping Pong stuff for timeout disconnects
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
                continue;

            if (pongDict.ContainsKey(m_Connections[i]))
            {
                if (Time.time - pongDict[m_Connections[i]].lastSendTime > 1f)
                {
                    pongDict[m_Connections[i]].lastSendTime = Time.time;
                    if (pongDict[m_Connections[i]].status == 0)
                    {
                        PlayerQuitMessage quitMsg = new PlayerQuitMessage();
                        quitMsg.networkID = playerInstances[m_Connections[i]].networkID;
                        HandleClientExit(this, m_Connections[i], quitMsg);
                    }
                    else
                    {
                        pongDict[m_Connections[i]].status -= 1;
                        PingMessage pingMsg = new PingMessage();
                        SendUnicast(m_Connections[i], pingMsg);
                    }
                }
            }
        }
    }

    static void HandleClientRegister(ServerBehaviour server, NetworkConnection con, BaseMessage header)
    {
        RegisterMessage message = header as RegisterMessage;
        server.StartCoroutine(server.RegisterPlayer(message.emailAddress, message.username, message.password, message.confirmPassword, con));
    }

    static void HandleClientHandshake(ServerBehaviour server, NetworkConnection con, BaseMessage header)
    {
        HandshakeMessage message = header as HandshakeMessage;
        server.StartCoroutine(server.LoginEnumerator(message.username, message.password, con));
    }

    public void SendUnicast(NetworkConnection connection, BaseMessage header, bool realiable = true)
    {
        DataStreamWriter writer;
        int result = m_Driver.BeginSend(realiable ? m_Pipeline : NetworkPipeline.Null, connection, out writer);
        if (result == 0)
        {
            header.SerializeObject(ref writer);
            m_Driver.EndSend(writer);
        }
    }

    public void SendBroadcast(BaseMessage header, NetworkConnection toExclude = default, bool realiable = true)
    {
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
                continue;

            DataStreamWriter writer;
            int result = m_Driver.BeginSend(m_Connections[i], out writer);
            if (result == 0)
            {
                header.SerializeObject(ref writer);
                m_Driver.EndSend(writer);
            }
        }
    }

    public void CallRPCOnClient(string function, NetworkedBehaviour target, params object[] data)
    {
        //if (gameStarted)
        //{
        RPCServerMessage rpc = new RPCServerMessage
        {
            target = target,
            methodName = function,
            data = data
        };
        SendBroadcast(rpc);
        //}
    }

    static void HandleClientRPCMessage(ServerBehaviour server, NetworkConnection con, BaseMessage header)
    {
        //if (server.gameStarted)
        //{
        RPCClientMessage msg = header as RPCClientMessage;
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
        //}
    }

    static void HandlePlayerReady(ServerBehaviour server, NetworkConnection con, BaseMessage header)
    {
        if (!server.gameStarted)
        {
            PlayerReadyMessage readyMsg = header as PlayerReadyMessage;
            //ready player instance
            //server.playerInstances[con].ready = true;

            //PlayerReadyResponseMessage spawnMsg = new PlayerReadyResponseMessage
            //{
            //    networkID = server.playerInstances[con].networkID,
            //};
            //server.SendBroadcast(spawnMsg, con);
            if (server.player1IDcon == server.m_Connections.IndexOf(con))
            {
                server.readyPlayer1 = !server.readyPlayer1;
            }
            else if (server.player2IDcon == server.m_Connections.IndexOf(con))
            {
                server.readyPlayer2 = !server.readyPlayer2;
            }
            if (server.readyPlayer1 && server.readyPlayer2)
            {
                StartGame(server, con, header);
            }
        }
    }

    static void StartGame(ServerBehaviour server, NetworkConnection con, BaseMessage header)
    {
        foreach (NetworkConnection item in server.playerInstances.Keys)
        {
            if (server.pongDict.ContainsKey(item))
            {
                server.pongDict.Remove(item);
            }
        }
        server.playerInstances.Clear();
        server.networkManager.ClearDict();

        NetworkedBehaviour player1;
        uint newNetworkID1 = NetworkedManager.NextNetworkID;
        NetworkConnection net1 = server.m_Connections[server.player1IDcon];
        if (server.networkManager.SpawnNewObjectWithId(NetworkSpawnObject.PLAYER, newNetworkID1, out player1))
        {
            // Get and setup player instance
            NetworkedPlayer playerInstance = player1.GetComponent<NetworkedPlayer>();
            playerInstance.isServer = true;
            playerInstance.isLocal = false;
            playerInstance.networkID = newNetworkID1;
            playerInstance.ownerID = server.player1ID;
            playerInstance.ready = true;
            playerInstance.transform.position = server.spawnLoc1;

            server.playerInstances.Add(net1, playerInstance);
        }
        StartGameMessage startMsg = new StartGameMessage
        {
            networkID = server.playerInstances[net1].networkID,
            objectType = NetworkSpawnObject.PLAYER,
            xPos = server.spawnLoc1.x,
            yPos = server.spawnLoc1.y,
        };
        server.SendUnicast(net1, startMsg);

        NetworkedBehaviour player2;
        uint newNetworkID2 = NetworkedManager.NextNetworkID;
        NetworkConnection net2 = server.m_Connections[server.player2IDcon];
        if (server.networkManager.SpawnNewObjectWithId(NetworkSpawnObject.PLAYER, newNetworkID2, out player2))
        {
            // Get and setup player instance
            NetworkedPlayer playerInstance = player2.GetComponent<NetworkedPlayer>();
            playerInstance.isServer = true;
            playerInstance.isLocal = false;
            playerInstance.networkID = newNetworkID2;
            playerInstance.ownerID = server.player2ID;
            playerInstance.ready = true;
            playerInstance.transform.position = server.spawnLoc2;

            server.playerInstances.Add(net2, playerInstance);
        }
        startMsg = new StartGameMessage
        {
            networkID = server.playerInstances[net2].networkID,
            objectType = NetworkSpawnObject.PLAYER,
            xPos = server.spawnLoc2.x,
            yPos = server.spawnLoc2.y,
        };
        server.SendUnicast(net2, startMsg);


        SpawnMessage spawnMsg = new SpawnMessage
        {
            networkID = server.playerInstances[net1].networkID,
            objectType = NetworkSpawnObject.PLAYER,
            xPos = server.spawnLoc1.x,
            yPos = server.spawnLoc1.y,
        };
        server.SendUnicast(net2, spawnMsg);

        spawnMsg = new SpawnMessage
        {
            networkID = server.playerInstances[net2].networkID,
            objectType = NetworkSpawnObject.PLAYER,
            xPos = server.spawnLoc2.x,
            yPos = server.spawnLoc2.y,
        };
        server.SendUnicast(net1, spawnMsg);
        server.gameStarted = true;

        if (Random.Range(0, 2) == 0)
        {
            server.currentPlayerTurn = newNetworkID1;
        }
        else
        {
            server.currentPlayerTurn = newNetworkID2;
        }
    }

    public void PlayerShooting()
    {
        if (currentPlayerTurn == playerInstances[m_Connections[player1IDcon]].networkID)
        {
            currentPlayerTurn = playerInstances[m_Connections[player2IDcon]].networkID;
        }
        else
        {
            currentPlayerTurn = playerInstances[m_Connections[player1IDcon]].networkID;
        }
    }

    static void HandleClientExit(ServerBehaviour server, NetworkConnection con, BaseMessage header)
    {
        PlayerQuitMessage quitMsg = header as PlayerQuitMessage;
        server.playerCount--;
        int winnerID = -1;
        int loserID = -1;
        if (server.player1IDcon == server.m_Connections.IndexOf(con))
        {
            winnerID = server.player2ID;
            loserID = server.player1ID;
            server.player1ID = -1;
            server.player1IDcon = -1;
            server.readyPlayer1 = false;
        }
        else if (server.player2IDcon == server.m_Connections.IndexOf(con))
        {
            winnerID = server.player1ID;
            loserID = server.player2ID;
            server.player2ID = -1;
            server.player2IDcon = -1;
            server.readyPlayer2 = false;
        }
        //if you join and quit quickly, might not be in this dict yet
        //if (server.pongDict.ContainsKey(con))
        //{
        server.pongDict.Remove(con);
        //}
        if (server.playerInstances.ContainsKey(con))
        {
            uint destroyId = quitMsg.networkID;
            server.networkManager.DestroyWithId(quitMsg.networkID);
            server.playerInstances.Remove(con);

            DestroyMessage destroyMsg = new DestroyMessage
            {
                networkID = destroyId
            };

            // Send Messages to all other clients
            server.SendBroadcast(destroyMsg, con);

            if (server.gameStarted)
            {
                server.EndGame(winnerID, loserID);
            }
            else
            {
                if (server.player1IDcon == server.m_Connections.IndexOf(con))
                {
                    server.readyPlayer1 = false;
                }
                else if (server.player2IDcon == server.m_Connections.IndexOf(con))
                {
                    server.readyPlayer2 = false;
                }
            }
        }
        else
        {
            Debug.LogError("Received exit from unlisted connection");
        }
        con.Disconnect(server.m_Driver);
        con = default;
    }

    public void EndGame(int _winnerID, int _loserID)
    {
        StartCoroutine(AddGameToDataBase(_winnerID, _loserID));
        gameStarted = false;
        readyPlayer1 = false;
        readyPlayer2 = false;
        //foreach (NetworkedPlayer player in playerInstances.Values)
        //{
        //    player.ready = false;
        //}
        //playerInstances.Clear();

        //send endgame message to players.
        string winnerName = "";
        if (_winnerID == player1ID)
        {
            winnerName = player1Name;
        }
        else
        {
            winnerName = player2Name;
        }
        foreach (NetworkConnection item in playerInstances.Keys)
        {
            StartCoroutine(GetWinLoseRatio(item, winnerName, playerInstances[item].ownerID));
        }
        //SendBroadcast(endMsg);
    }

    IEnumerator GetWinLoseRatio(NetworkConnection con, string _winnerName, int _playerID, int _gameID = 6)
    {
        WWWForm form = new WWWForm();
        form.AddField("session_id", sessionID);
        form.AddField("game_id", _gameID);
        form.AddField("player_id", _playerID);

        UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~maxym.ebeling/WinLoseRatio.php", form);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            WinLoseResponse response = JsonUtility.FromJson<WinLoseResponse>(www.downloadHandler.text);
            EndGameMessage endMsg = new EndGameMessage();
            endMsg.winnerName = _winnerName;
            endMsg.wins = response.wins;
            endMsg.loses = response.loses;
            SendUnicast(con, endMsg);
        }
    }

    IEnumerator AddGameToDataBase(int _winnerID, int _loserID, int _gameID = 6)
    {
        WWWForm form = new WWWForm();
        form.AddField("session_id", sessionID);
        form.AddField("game_id", _gameID);
        form.AddField("winner_id", _winnerID);
        form.AddField("loser_id", _loserID);

        UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~maxym.ebeling/GamePlayed.php", form);

        yield return www.SendWebRequest();
        //string result = Encoding.UTF8.GetString(www.downloadHandler.data);
        //JsonUtility.FromJson<RegisterResponse>(www.text);
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("GAME ADDED TO DATABASE");
        }
    }

    IEnumerator RegisterPlayer(string emailAddress, string username, string password, string confirmPassword, NetworkConnection con)
    {
        WWWForm form = new WWWForm();
        form.AddField("session_id", sessionID);
        form.AddField("email", emailAddress);
        form.AddField("username", username);
        form.AddField("password", password);
        form.AddField("confirmPassword", confirmPassword);

        using (UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~maxym.ebeling/Register.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log(responseText);
                if (responseText.StartsWith("\"Success\""))
                {
                    Debug.Log("Account Registered");
                    //loginScreen.error.text = "Registration Complete";
                    //BackToLoginScreen();
                    RegisterResponseMessage responseMsg = new RegisterResponseMessage();
                    responseMsg.username = username;
                    responseMsg.password = password;
                    SendUnicast(con, responseMsg);
                }
                else
                {
                    //error.text = responseText;
                }
            }
        }
    }

    IEnumerator LoginEnumerator(string username, string password, NetworkConnection con)
    {
        WWWForm form = new WWWForm();
        form.AddField("session_id", sessionID);
        form.AddField("username", username);
        form.AddField("password", password);

        UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~maxym.ebeling/Login.php", form);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string responseText = www.downloadHandler.text;
            if (responseText.StartsWith("Success"))
            {
                Debug.Log("USER LOGGED IN");
                string[] dataChunks = responseText.Split('|');
                var newPlayerID = dataChunks[1];
                //userEmail = dataChunks[2];
                //isLoggedIn = true;

                //ResetValues();
                if (playerCount < 2)
                {
                    playerCount++;
                    if (player1IDcon == -1)
                    {
                        player1IDcon = m_Connections.IndexOf(con);
                        player1ID = int.Parse(newPlayerID);
                        player1Name = username;
                    }
                    else if (player2IDcon == -1)
                    {
                        player2IDcon = m_Connections.IndexOf(con);
                        player2ID = int.Parse(newPlayerID);
                        player2Name = username;
                    }
                    HandshakeResponseMessage responseMsg = new HandshakeResponseMessage();
                    responseMsg.connected = 1;
                    responseMsg.responseText = username;
                    SendUnicast(con, responseMsg);

                    //means they've succesfully handshaked
                    PingPong ping = new PingPong();
                    ping.lastSendTime = Time.time;
                    ping.status = 3;    // 3 retries
                    pongDict.Add(con, ping);

                    PingMessage pingMsg = new PingMessage();
                    SendUnicast(con, pingMsg);

                    // Send all existing players to this player
                    foreach (KeyValuePair<NetworkConnection, NetworkedPlayer> pair in playerInstances)
                    {
                        if (pair.Key == con) continue;

                        SpawnMessage spawnMsg = new SpawnMessage
                        {
                            networkID = pair.Value.networkID,
                            objectType = NetworkSpawnObject.PLAYER
                        };

                        SendUnicast(con, spawnMsg);
                    }
                }
                else
                {
                    HandshakeResponseMessage responseMsg = new HandshakeResponseMessage();
                    responseMsg.connected = 0;
                    responseMsg.responseText = "ROOM IS FULL";
                    SendUnicast(con, responseMsg);
                }

            }
            else
            {
                HandshakeResponseMessage responseMsg = new HandshakeResponseMessage();
                responseMsg.connected = 0;
                responseMsg.responseText = responseText;
                SendUnicast(con, responseMsg);
                Debug.Log(www.downloadHandler.text);
                Debug.Log("ERROR LOGGING IN");
                //error.text = responseText;
            }
        }

    }

    IEnumerator GetSessionID()
    {
        WWWForm form = new WWWForm();
        form.AddField("session_id", sessionID);
        form.AddField("server_id", serverID);
        form.AddField("password", serverPassword);

        UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~maxym.ebeling/Server_Login.php", form);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            sessionID = www.downloadHandler.text;
            StartServer();
        }
    }

    void StartServer()
    {
        IPAddress serverAddress = IPAddress.Parse("217.105.54.134");                                                //????
        NativeArray<byte> nativeArrayAddress;                                                                       //????
        // Convert that into a NativeArray of byte data                                                             //????
        nativeArrayAddress = new NativeArray<byte>(serverAddress.GetAddressBytes().Length, Allocator.Temp);         //????
        nativeArrayAddress.CopyFrom(serverAddress.GetAddressBytes());                                               //????

        m_Driver = NetworkDriver.Create();
        m_Pipeline = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.SetRawAddressBytes(nativeArrayAddress);         // Use SetRawAddressBytes                          //????
        endpoint.Port = 9000;
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

    }

    static void HandleClientPong(ServerBehaviour server, NetworkConnection con, BaseMessage header)
    {
        server.pongDict[con].status = 3;   //reset retry count
    }
}

public class WinLoseResponse
{
    public int wins;
    public int loses;
}