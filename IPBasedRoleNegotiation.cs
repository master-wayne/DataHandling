using Mirror;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class IPBasedRoleNegotiation : MonoBehaviour
{
    public NetworkManager networkManager;

    private UdpClient udpClient;
    private IPEndPoint broadcastEndPoint;
    private string localIP;
    private string lowestIP = null;
    private bool decisionMade = false;
    private float discoveryTime = 2f; // Time to wait for IP discovery

    void Start()
    {
        if (networkManager == null)
        {
            networkManager = GetComponent<NetworkManager>();
        }

        // Get local IP address
        localIP = GetLocalIPAddress();
        Debug.Log($"Local IP: {localIP}");

        // Setup UDP for broadcasting and listening
        udpClient = new UdpClient(8888); // Use a specific port for UDP
        broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 8888);

        // Start listening for other IPs
        udpClient.BeginReceive(OnUdpReceive, null);

        // Broadcast our IP repeatedly during discovery period
        InvokeRepeating(nameof(BroadcastIP), 0f, 0.5f);

        // Decide role after discovery period
        Invoke(nameof(DecideRole), discoveryTime);
    }

    // Get the local machine's IP address
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
        throw new Exception("No IPv4 address found!");
    }

    // Broadcast this instance's IP address
    void BroadcastIP()
    {
        byte[] ipBytes = Encoding.UTF8.GetBytes(localIP);
        udpClient.Send(ipBytes, ipBytes.Length, broadcastEndPoint);
    }

    // Receive IPs from other instances
    void OnUdpReceive(IAsyncResult result)
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receivedBytes = udpClient.EndReceive(result, ref remoteEndPoint);
        string receivedIP = Encoding.UTF8.GetString(receivedBytes);

        // Only process if we haven't decided yet
        if (!decisionMade && receivedIP != localIP) // Ignore our own broadcast
        {
            Debug.Log($"Received IP: {receivedIP}");

            // Update lowest IP if this is the first or a lower one
            if (lowestIP == null || String.Compare(receivedIP, lowestIP) < 0)
            {
                lowestIP = receivedIP;
            }
        }

        // Continue listening
        udpClient.BeginReceive(OnUdpReceive, null);
    }

    // Decide whether to be host or client based on IP comparison
    void DecideRole()
    {
        CancelInvoke(nameof(BroadcastIP)); // Stop broadcasting
        decisionMade = true;

        // If no lower IP was found, or our IP is the lowest, become the host
        if (lowestIP == null || String.Compare(localIP, lowestIP) < 0)
        {
            Debug.Log("This instance has the lowest IP. Starting as Host.");
            networkManager.StartHost();
        }
        else
        {
            Debug.Log($"Connecting to host with lower IP: {lowestIP}");
            networkManager.networkAddress = lowestIP;
            networkManager.StartClient();
        }

        // Clean up UDP client
        udpClient.Close();
    }

    void OnDestroy()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}