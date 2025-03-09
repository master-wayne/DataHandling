using kcp2k;
using Mirror;
using UnityEngine;

public class HostSwitchManager : NetworkBehaviour
{
    private NetworkManagerCustom networkManager;
    private string localIP;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        networkManager = FindObjectOfType<NetworkManagerCustom>();
        localIP = GetLocalIPAddress();
    }

    public void HandleHostDiscovery(string discoveredHostIP, int myPort)
    {
        if (!isLocalPlayer) return; // Only the local player handles this

        if (!string.IsNullOrEmpty(discoveredHostIP))
        {
            int comparison = CompareIPAddresses(localIP, discoveredHostIP);
            if (comparison >= 0) // Our IP is lower
            {
                networkManager.StopHost();
                networkManager.networkAddress = discoveredHostIP;
                networkManager.StartClient();
            }
            else if (comparison < 0) // Our IP is higher
            {
                if (isServer) // Ensure we're already hosting
                {
                    CmdNotifyHostChange(discoveredHostIP, localIP, myPort);
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdNotifyHostChange(string oldHostIP, string newHostIP, int newHostPort)
    {
        if (CompareIPAddresses(localIP, oldHostIP) == 0) // If we're the old host
        {
            networkManager.StopHost();
            networkManager.networkAddress = newHostIP;
            networkManager.GetComponent<KcpTransport>().port = (ushort)newHostPort;
            networkManager.StartClient();
        }

        RpcSwitchToNewHost(newHostIP, newHostPort);
    }

    [ClientRpc]
    private void RpcSwitchToNewHost(string newHostIP, int newHostPort)
    {
        if (!NetworkServer.active) // Only for clients, not the host
        {
            networkManager.StopClient();
            networkManager.networkAddress = newHostIP;
            networkManager.GetComponent<KcpTransport>().port = (ushort)newHostPort;
            networkManager.StartClient();
        }
    }

    private string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "0.0.0.0";
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