using UnityEngine;

public class NetworkedPlayer : NetworkedBehaviour
{
    public float moveSpeed;
    Vector2 movementDirection;
    Rigidbody2D rb;
    public bool ready;
    Animator anim;

    Vector2 toLocation;
    public Transform playerArt;
    public Transform gunTransform;
    float zRot;
    public int ownerID;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (isLocal)
        {
            Camera.main.GetComponent<CameraFollowPlayer>().SetTarget(transform);
        }
        if (isServer)
        {
            server = FindObjectOfType<ServerBehaviour>();
        }
        else
        {
            client = FindObjectOfType<ClientBehaviour>();
            anim = GetComponentInChildren<Animator>();
            toLocation = transform.position;
        }
    }

    private void Update()
    {
        if (ready)
        {
            if (isLocal)
            {
                client.CallRPCOnServer("MovementInput", this, Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), Aim());
                //try sending RPCFire when firing...
                if (Input.GetMouseButtonDown(0))
                {
                    client.CallRPCOnServer("Fire", this, 0, transform.position.x, transform.position.y, zRot);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (isServer)
        {
            if (ready)
            {
                int animIndex = int.MaxValue;
                if (movementDirection != Vector2.zero)
                {
                    animIndex = 0;
                    if (movementDirection.x > 0)
                    {
                        animIndex = 1;
                    }
                    else if (movementDirection.x < 0)
                    {
                        animIndex = -1;
                    }
                }

                // TODO: Send position update to all clients (maybe not every frame!)
                //if (Time.frameCount % 3 == 0)
                //{ // assuming 60fps, so 20fps motion updates
                // We could consider sending this over a non-reliable pipeline
                gunTransform.rotation = Quaternion.Euler(0, 0, zRot);
                server.CallRPCOnClient("Move", this, transform.position.x, transform.position.y, movementDirection.x, movementDirection.y, animIndex, zRot);
                //}
                rb.MovePosition(rb.position + new Vector2(movementDirection.x * Time.fixedDeltaTime * moveSpeed, movementDirection.y * Time.fixedDeltaTime * moveSpeed));
            }
        }
        else
        {
            toLocation += new Vector2(movementDirection.x * Time.fixedDeltaTime * moveSpeed, movementDirection.y * Time.fixedDeltaTime * moveSpeed);
            rb.MovePosition(Vector2.Lerp(rb.position, toLocation, 0.75f));
        }

    }

    float Aim()
    {
        var aimDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        return Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        //gunTransform.rotation = Quaternion.Euler(0, 0, rotZ);
    }

    public void Fire(int bulletID, float xPos, float yPos, float _zRot)
    {
        if (isServer)
        {
            if (server.currentPlayerTurn == networkID)
            {
                server.PlayerShooting();
                NetworkedBehaviour obj;
                uint id = NetworkedManager.NextNetworkID;
                if (server.networkManager.SpawnNewObjectWithId(NetworkSpawnObject.BULLET, id, out obj))
                {
                    obj.GetComponent<NetworkedBullet>().isServer = true;
                    obj.GetComponent<NetworkedBullet>().ownerIDcon = networkID;
                    obj.GetComponent<NetworkedBullet>().ownerID = ownerID;
                    obj.transform.position = transform.position;
                    obj.transform.rotation = Quaternion.Euler(0, 0, zRot);
                    server.CallRPCOnClient("Fire", this, (int)id, transform.position.x, transform.position.y, zRot);
                }
            }
        }
        else
        {
            NetworkedBehaviour obj;
            client.networkManager.SpawnNewObjectWithId(NetworkSpawnObject.BULLET, (uint)bulletID, out obj);
            obj.transform.position = new Vector3(xPos, yPos, 0);
            obj.transform.rotation = Quaternion.Euler(0, 0, _zRot);
            anim.SetTrigger("Attack");
            Debug.Log("TESETS");
        }

    }

    public void MovementInput(float moveX, float moveY, float _zRot)
    {
        if (isServer)
        {
            movementDirection = new Vector2(Mathf.Clamp(moveX, -1f, 1f), Mathf.Clamp(moveY, -1f, 1f));
            zRot = _zRot;
        }
    }

    public void Move(float xPos, float yPos, float moveX, float moveY, int animIndex, float _zRot)
    {
        toLocation = new Vector2(xPos, yPos);
        //transform.position = new Vector2(xPos, yPos);
        if (animIndex == int.MaxValue)
        {
            anim.SetBool("IsRunning", false);
        }
        else
        {
            anim.SetBool("IsRunning", true);
            if (animIndex != 0)
            {
                playerArt.localScale = new Vector3(animIndex, 1, 1);
            }
        }
        movementDirection = new Vector2(moveX, moveY);
        gunTransform.rotation = Quaternion.Euler(0, 0, _zRot);
    }

    public void DestroyPlayer()
    {
        ready = false;
        movementDirection = Vector2.zero;
        gunTransform.gameObject.SetActive(false);
        if (isServer)
        {
            //server.networkManager.DestroyWithId(networkID);
        }
        else
        {
            //client.networkManager.DestroyWithId(networkID);
            anim.SetTrigger("Die");
        }
    }
}
