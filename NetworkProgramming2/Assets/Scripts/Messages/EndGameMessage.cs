using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class EndGameMessage : BaseMessage
{
    public override NetworkMessageType Type
    {
        get
        {
            return NetworkMessageType.END_GAME;
        }
    }

    public string winnerName;
    public int wins;
    public int loses;
    public int totalGames;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);
        writer.WriteFixedString32(winnerName);
        writer.WriteInt(wins);
        writer.WriteInt(loses);
        writer.WriteInt(totalGames);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);
        winnerName = reader.ReadFixedString32().ToString();
        wins = reader.ReadInt();
        loses = reader.ReadInt();
        totalGames = reader.ReadInt();
    }
}
