using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class DestroyMessage : BaseMessage
{
    public override NetworkMessageType Type
    {
        get
        {
            return NetworkMessageType.DESTROY;
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
