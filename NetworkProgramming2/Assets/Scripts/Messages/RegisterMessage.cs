using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class RegisterMessage : BaseMessage
{
    public override NetworkMessageType Type
    {
        get
        {
            return NetworkMessageType.REGISTER;
        }
    }

    public string emailAddress;
    public string username;
    public string password;
    public string confirmPassword;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);
        writer.WriteFixedString32(emailAddress);
        writer.WriteFixedString32(username);
        writer.WriteFixedString32(password); 
        writer.WriteFixedString32(confirmPassword);

    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);
        emailAddress = reader.ReadFixedString32().ToString();
        username = reader.ReadFixedString32().ToString();
        password = reader.ReadFixedString32().ToString();
        confirmPassword = reader.ReadFixedString32().ToString();
    }
}
