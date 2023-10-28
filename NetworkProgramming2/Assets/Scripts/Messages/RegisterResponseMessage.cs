using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class RegisterResponseMessage : BaseMessage
{
    public override NetworkMessageType Type
    {
        get
        {
            return NetworkMessageType.REGISTER_RESPONSE;
        }
    }

    public string username;
    public string password;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);
        writer.WriteFixedString32(username);
        writer.WriteFixedString32(password);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);
        username = reader.ReadFixedString32().ToString();
        password = reader.ReadFixedString32().ToString();
    }
}
