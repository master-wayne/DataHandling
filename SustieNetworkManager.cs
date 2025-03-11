using System.Collections;
using System.Collections.Generic;
using System.Net;
using Mirror;
using UnityEngine;

public class SustieNetworkManager : NetworkManager
{
    public override void Start()
    {
        base.Start();
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.Log("Host started!");
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        Debug.Log("Host stopped!");
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
    }
}
