using System.Collections;
using System.Collections.Generic;
using System.Net;
using Mirror;
using UnityEngine;

public class SustieNetworkCalls : NetworkBehaviour
{
    private AutoLANNetworkDiscovery networkDiscovery;

    [SyncVar]
    public string hostIP = "";

    [SyncVar]
    public List<string> ClientIPs = new();

    private void Start()
    {
        networkDiscovery = FindObjectOfType<AutoLANNetworkDiscovery>();
    }

    private void Update()
    {
        if (isServer)
        {
            hostIP = GetLocalIPAddress();
            UpdateClientIPList();
            foreach(string ip in ClientIPs)
            {
                Debug.Log(ip);
            }
        }
        if (isClient)
        {
            string clientIP = GetLocalIPAddress();
            int comparison = CompareIPAddresses(clientIP, hostIP);
            if (comparison <= 0)
            {
                //StopHost();
            }
        }
    }

    [TargetRpc]
    public void TurnIntoHost()
    {

    }

    [TargetRpc]
    public void ConnectTo()
    {

    }

    [Command]
    public void StopHost()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            Debug.Log("Called!");
            networkDiscovery.StopDiscovery();
            StartCoroutine(StopHostRoutine());
        }
    }

    private IEnumerator StopHostRoutine()
    {
        yield return new WaitForEndOfFrame();
        SustieNetworkManager.singleton.StopHost();
    }

    [Command(requiresAuthority = false)]
    public void UpdateClientIPList()
    {
        ClientIPs.Clear();
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            ClientIPs.Add(conn.address);
        }
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return null;
    }

    private int CompareIPAddresses(string ipA, string ipB)
    {
        byte[] bytesA = System.Net.IPAddress.Parse(ipA).GetAddressBytes();
        byte[] bytesB = System.Net.IPAddress.Parse(ipB).GetAddressBytes();

        for (int i = 0; i < bytesA.Length; i++)
        {
            if (bytesA[i] != bytesB[i])
            {
                return bytesA[i].CompareTo(bytesB[i]);
            }
        }
        return 0;
    }

}
