using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Networking.Transport;
using UnityEngine;

public class RPCServerMessage : BaseMessage
{
    static NetworkedManager networkManager;

    public override NetworkMessageType Type
    {
        get
        {
            return NetworkMessageType.RPC_SERVER;
        }
    }

    public NetworkedBehaviour target;
    public string methodName;
    public object[] data;

    public MethodInfo mInfo;
    public ParameterInfo[] parameters;

    public uint networkID;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteUInt((uint)target.networkID);
        writer.WriteFixedString128(methodName);

        // need MethodInfo to serialize
        mInfo = target.GetType().GetMethod(methodName);
        if (mInfo == null)
        {
            throw new System.ArgumentException($"Object of type {target.GetType()} does not contain method called {methodName}");
        }

        // Get Function Parameters, and write them one in order
        parameters = mInfo.GetParameters();
        for (int i = 0; i < parameters.Length; ++i)
        {
            if (parameters[i].ParameterType == typeof(string))
            {
                writer.WriteFixedString128((string)data[i]);
            }
            else if (parameters[i].ParameterType == typeof(float))
            {
                writer.WriteFloat((float)data[i]);
            }
            else if (parameters[i].ParameterType == typeof(int))
            {
                writer.WriteInt((int)data[i]);
            }
            else if (parameters[i].ParameterType == typeof(Vector3))
            {
                Vector3 vec = (Vector3)data[i];
                writer.WriteFloat(vec.x);
                writer.WriteFloat(vec.y);
                writer.WriteFloat(vec.z);
            }
            else
            {
                throw new System.ArgumentException($"Unhandled RPC Type: {parameters[i].ParameterType.ToString()}");
            }
        }
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        // very important to call this first, otherwise the base-class data will be at the end (it needs to be at the start)
        base.DeserializeObject(ref reader);

        networkID = reader.ReadUInt();
        methodName = reader.ReadFixedString128().ToString();

        //TODO: Find a better way to do this...
        if (networkManager == null) networkManager = Object.FindObjectOfType<NetworkedManager>();

        // Get Object reference, and extract the MethodInfo from it's NetworkedBehaviour
        //	-> this works because the object itself knows its type (e.g. PlayerBehaviour), even if we treat it as its base (NetworkedBehaviour)
        NetworkedBehaviour obj;
        if (networkManager.GetObject(networkID, out obj))
        {
            target = obj;
            mInfo = target.GetType().GetMethod(methodName);
            if (mInfo == null)
            {
                throw new System.ArgumentException($"Object of type {target.GetType()} does not contain method called {methodName}");
            }
        }
        else
        {
            Debug.LogError($"Could not find object with id {networkID}");
        }

        // Extract Parameter info
        parameters = mInfo.GetParameters();

        // Prepare Data to store parameters into
        data = new object[parameters.Length];

        // Read each parameter in order (must be the same as we wrote it)
        for (int i = 0; i < parameters.Length; ++i)
        {
            if (parameters[i].ParameterType == typeof(string))
            {
                data[i] = reader.ReadFixedString128().ToString();
            }
            else if (parameters[i].ParameterType == typeof(float))
            {
                data[i] = reader.ReadFloat();
            }
            else if (parameters[i].ParameterType == typeof(int))
            {
                data[i] = reader.ReadInt();
            }
            else if (parameters[i].ParameterType == typeof(Vector3))
            {
                data[i] = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
            }
            else
            {
                throw new System.ArgumentException($"Unhandled RPC Type: {parameters[i].ParameterType.ToString()}");
            }
        }
    }
}
