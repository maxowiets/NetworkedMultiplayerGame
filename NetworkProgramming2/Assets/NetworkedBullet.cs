using Unity.Burst.CompilerServices;
using UnityEngine;

public class NetworkedBullet : NetworkedBehaviour
{
    public float bulletSpeed;
    public float hitRadius;
    public uint ownerIDcon;
    public int ownerID;

    public GameObject particle;
    Vector2 toLocation;

    private void Start()
    {
        if (isServer)
        {
            server = FindObjectOfType<ServerBehaviour>();
            Invoke("DestroyBullet", 10f);
        }
        else
        {
            client = FindObjectOfType<ClientBehaviour>();
            GameManager.Instance.soundManager.PlayAudio(0, transform.position);
            toLocation = transform.position;
        }
    }

    private void Update()
    {
        //transform.Translate(transform.right * bulletSpeed * Time.deltaTime);
        if (isServer)
        {
            transform.position += bulletSpeed * Time.deltaTime * transform.right;
            server.CallRPCOnClient("MoveBullet", this, transform.position.x, transform.position.y);

            var hits = Physics2D.CircleCastAll(transform.position, hitRadius, transform.right, bulletSpeed * Time.deltaTime);
            foreach (var hit in hits)
            {
                if (hit.collider.GetComponent<NetworkedPlayer>())
                {
                    if (hit.collider.GetComponent<NetworkedPlayer>().networkID != ownerIDcon)
                    {
                        int winnerID = ownerID;
                        int loserID = hit.collider.GetComponent<NetworkedPlayer>().ownerID;
                        //Destroy Player
                        //Destroy Bullet
                        Debug.Log("playerhit");
                        hit.collider.GetComponent<NetworkedPlayer>().DestroyPlayer();
                        server.CallRPCOnClient("DestroyPlayer", hit.collider.GetComponent<NetworkedPlayer>());
                        server.EndGame(winnerID, loserID);
                        DestroyBullet();
                    }
                }
                else if (hit.collider.GetComponent<Obstacle>())
                {
                    DestroyBullet();
                }
            }
        }
        else
        {
            toLocation += bulletSpeed * Time.deltaTime * (Vector2)transform.right;
            transform.position = Vector2.Lerp(transform.position, toLocation, 0.75f);
        }
    }

    public void DestroyBullet()
    {
        if (isServer)
        {
            server.CallRPCOnClient("DestroyBullet", this);
            server.networkManager.DestroyWithId(networkID);
        }
        else
        {
            client.networkManager.DestroyWithId(networkID);
            Instantiate(particle, transform.position, Quaternion.identity);
        }
    }

    public void MoveBullet(float xPos, float yPos)
    {
        toLocation = new Vector2 (xPos, yPos);
    }
}
