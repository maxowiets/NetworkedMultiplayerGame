using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedManager : MonoBehaviour
{
    private static uint nextNetworkId = 0;
    public static uint NextNetworkID => ++nextNetworkId;

    [SerializeField] private NetworkSpawnInfo spawnInfo;
    private Dictionary<uint, NetworkedBehaviour> networkedObjects = new Dictionary<uint, NetworkedBehaviour>();

    public bool GetObject(uint id, out NetworkedBehaviour obj)
    {
        obj = null;
        if (networkedObjects.ContainsKey(id))
        {
            obj = networkedObjects[id];
            return true;
        }
        return false;
    }

    public bool SpawnNewObjectWithId(NetworkSpawnObject type, uint id, out NetworkedBehaviour obj)
    {
        obj = null;
        if (networkedObjects.ContainsKey(id))
        {
            return false;
        }
        else
        {
            // assuming this doesn't crash...
            obj = Instantiate(spawnInfo.prefabList[(int)type]);

            NetworkedBehaviour beh = obj.GetComponent<NetworkedBehaviour>();
            if (beh == null)
            {
                beh = obj.gameObject.AddComponent<NetworkedBehaviour>();
            }
            beh.networkID = id;

            networkedObjects.Add(id, obj);

            return true;
        }
    }

    public bool DestroyWithId(uint id)
    {
        if (networkedObjects.ContainsKey(id))
        {
            Destroy(networkedObjects[id].gameObject);
            networkedObjects.Remove(id);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ClearDict()
    {
        if (networkedObjects.Count > 0)
        {
            foreach (var item in networkedObjects.Keys)
            {
                Destroy(networkedObjects[item].gameObject);
            }
            networkedObjects.Clear();
            //networkedObjects = new Dictionary<uint, NetworkedBehaviour>();
        }
    }
}
