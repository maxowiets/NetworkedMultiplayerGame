using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class PlayerQuitMessage : BaseMessage
{
    public override NetworkMessageType Type
    {
        get
        {
            return NetworkMessageType.PLAYER_QUIT;
        }
    }

    public uint networkID;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);
        writer.WriteUInt(networkID);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);
        networkID = reader.ReadUInt();
    }
}
