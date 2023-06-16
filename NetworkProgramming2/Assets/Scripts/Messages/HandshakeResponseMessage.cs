using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class HandshakeResponseMessage : BaseMessage
{
    public override NetworkMessageType Type 
    {
        get 
        {
            return NetworkMessageType.HANDSHAKE_RESPONSE;
        } 
    }
    public uint connected;
    public string responseText;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);
        writer.WriteUInt(connected);
        writer.WriteFixedString32(responseText);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);
        connected = reader.ReadUInt();
        responseText = reader.ReadFixedString32().ToString();
    }
}
