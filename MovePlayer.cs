using UnityEngine;
using Mirror;

public class MovePlayer : NetworkBehaviour
{
    [SerializeField]
    public float moveSpeed = 5f;

    public override void OnStartLocalPlayer()
    {
        GetComponent<Renderer>().material.color = Random.ColorHSV();
    }

    void Update()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;
        transform.position += move;

        CmdMove(transform.position);
    }

    [Command]
    void CmdMove(Vector3 newPosition)
    {
        transform.position = newPosition;
        RpcSyncPosition(newPosition);
    }
    [ClientRpc]
    void RpcSyncPosition(Vector3 newPosition)
    {
        if(!isLocalPlayer)
        {
            transform.position = newPosition;
        }
    }
}
