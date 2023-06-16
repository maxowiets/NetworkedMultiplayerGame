using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class StartGameMessage : BaseMessage
{
    public override NetworkMessageType Type
    {
        get
        {
            return NetworkMessageType.START_GAME;
        }
    }

    public NetworkSpawnObject objectType;
    public float xPos;
    public float yPos;
    public uint networkID;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);
        writer.WriteUInt(networkID);
        writer.WriteUInt((uint)objectType);
        writer.WriteFloat(xPos);
        writer.WriteFloat(yPos);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);
        networkID = reader.ReadUInt();
        objectType = (NetworkSpawnObject)reader.ReadUInt();
        xPos = reader.ReadFloat();
        yPos = reader.ReadFloat();
    }
}
