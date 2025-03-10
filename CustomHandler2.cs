using UnityEngine;
using Mirror;
using Mirror.Discovery;
using System.Net;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(NetworkManager), typeof(NetworkDiscovery))]
public class NetworkRoleManager : MonoBehaviour
{
    private NetworkManager networkManager;
    private NetworkDiscovery networkDiscovery;

    private string myIP;
    private Dictionary<string, ServerResponse> discoveredServers = new Dictionary<string, ServerResponse>();
    private bool isHost = false;

    void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        networkDiscovery = GetComponent<NetworkDiscovery>();
        myIP = GetLocalIPAddress();
        Debug.Log("My IP: " + myIP);

        // Setup discovery callbacks
        networkDiscovery.OnServerFound.AddListener(OnDiscoveredServer);
    }

    void Start()
    {
        // Start discovery immediately
        StartDiscovery();
    }

    void StartDiscovery()
    {
        discoveredServers.Clear();
        networkDiscovery.StopDiscovery();
        networkDiscovery.StartDiscovery(); // Listen for other servers
        Invoke("DecideRole", 2f); // Give time to discover others
    }

    void OnDiscoveredServer(ServerResponse response)
    {
        string serverIP = response.EndPoint.Address.ToString();
        if (!discoveredServers.ContainsKey(serverIP) && serverIP != myIP)
        {
            discoveredServers[serverIP] = response;
            Debug.Log($"Discovered server at {serverIP}");
        }
    }

    void DecideRole()
    {
        // Include my IP in the comparison
        List<string> allIPs = new List<string>(discoveredServers.Keys) { myIP };
        allIPs.Sort(); // Sort IPs lexicographically (works for simple IPv4 comparison)

        string lowestIP = allIPs[0];
        Debug.Log($"Lowest IP detected: {lowestIP}");

        if (myIP == lowestIP)
        {
            // I have the lowest IP, become the host
            if (!isHost)
            {
                StopAllConnections();
                networkManager.StartHost();
                networkDiscovery.AdvertiseServer(); // Broadcast as host
                isHost = true;
                Debug.Log("Became host with IP: " + myIP);
            }
        }
        else
        {
            // Connect as client to the lowest IP
            if (isHost)
            {
                StopAllConnections();
                isHost = false;
            }
            ConnectAsClient(lowestIP);
        }

        // Continue discovery to handle new joiners
        Invoke("StartDiscovery", 5f); // Check periodically
    }

    void ConnectAsClient(string targetIP)
    {
        if (NetworkClient.isConnected && networkManager.networkAddress == targetIP)
            return; // Already connected to the correct host

        StopAllConnections();
        networkManager.networkAddress = targetIP;
        networkManager.StartClient();
        Debug.Log($"Connecting as client to {targetIP}");
    }

    void StopAllConnections()
    {
        if (NetworkServer.active || NetworkClient.active)
        {
            networkManager.StopHost();
            networkManager.StopClient();
            Debug.Log("Stopped all connections.");
        }
    }

    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4
            {
                return ip.ToString();
            }
        }
        return "0.0.0.0";
    }

    void OnApplicationQuit()
    {
        StopAllConnections();
        networkDiscovery.StopDiscovery();
    }
}