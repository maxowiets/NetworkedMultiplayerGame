using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public abstract class BaseMessage
{
    private static uint nextID = 0;
    public static uint NextID => ++nextID;
    public abstract NetworkMessageType Type { get; }
    public uint ID { get; private set; } = NextID;

    public virtual void SerializeObject(ref DataStreamWriter writer)
    {
        writer.WriteUShort((ushort)Type);
        writer.WriteUInt(ID);
    }

    public virtual void DeserializeObject(ref DataStreamReader reader)
    {
        ID = reader.ReadUInt();
    }
}

public static class NetworkMessageInfo
{
    public static Dictionary<NetworkMessageType, System.Type> TypeMap = new Dictionary<NetworkMessageType, System.Type> {
            { NetworkMessageType.HANDSHAKE,                 typeof(HandshakeMessage) },
            { NetworkMessageType.HANDSHAKE_RESPONSE,        typeof(HandshakeResponseMessage) },            
            { NetworkMessageType.PLAYER_READY,              typeof(PlayerReadyMessage) },
            { NetworkMessageType.PLAYER_READY_RESPONSE,     typeof(PlayerReadyResponseMessage) },
            { NetworkMessageType.START_GAME,                typeof(StartGameMessage) },
            { NetworkMessageType.NETWORK_SPAWN,             typeof(SpawnMessage) },
            { NetworkMessageType.RPC_CLIENT,                typeof(RPCClientMessage) },
            { NetworkMessageType.RPC_SERVER,                typeof(RPCServerMessage) },
            { NetworkMessageType.PLAYER_QUIT,               typeof(PlayerQuitMessage) },
            { NetworkMessageType.DESTROY,                   typeof(DestroyMessage) },
            { NetworkMessageType.END_GAME,                  typeof(EndGameMessage) },
            { NetworkMessageType.PING,                      typeof(PingMessage) },
            { NetworkMessageType.PONG,                      typeof(PongMessage) }
    };
}

public enum NetworkMessageType
{
    HANDSHAKE,
    HANDSHAKE_RESPONSE,
    //CHAT_MESSAGE,
    PLAYER_READY,
    PLAYER_READY_RESPONSE,
    START_GAME,
    NETWORK_SPAWN,
    //NETWORK_DESTROY,
    //NETWORK_UPDATE_POSITION,
    RPC_SERVER,
    RPC_CLIENT,
    PLAYER_QUIT,
    DESTROY,
    END_GAME,
    //INPUT_UPDATE,                        // uint networkId, InputUpdate (float, float, bool)
    PING,
    PONG
}