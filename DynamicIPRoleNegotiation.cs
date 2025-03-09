using Mirror;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class DynamicIPRoleNegotiation : MonoBehaviour
{
    public CustomNetworkManager1 networkManager;

    private UdpClient udpClient;
    private IPEndPoint broadcastEndPoint;
    private string localIP;
    private string currentHostIP = null;
    private float broadcastInterval = 1f;
    private float hostTimeout = 3f;
    private float lastHostPingTime;
    private bool isHost = false;

    public bool IsHost => isHost;

    void Start()
    {
        if (networkManager == null)
        {
            networkManager = GetComponent<CustomNetworkManager1>();
        }

        localIP = GetLocalIPAddress();
        Debug.Log($"Local IP: {localIP}");

        try
        {
            udpClient = new UdpClient(8888);
            udpClient.EnableBroadcast = true;
            broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 8888);
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP Setup Error: {e.Message}");
            return;
        }

        udpClient.BeginReceive(OnUdpReceive, null);
        InvokeRepeating(nameof(BroadcastIP), 0f, broadcastInterval);
        Invoke(nameof(InitialRoleDecision), 2f);

        networkManager.roleNegotiator = this;
    }

    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No IPv4 address found!");
    }

    void BroadcastIP()
    {
        string message = $"{localIP}:{(isHost ? "HOST" : "CLIENT")}";
        byte[] ipBytes = Encoding.UTF8.GetBytes(message);
        udpClient.Send(ipBytes, ipBytes.Length, broadcastEndPoint);
    }

    void OnUdpReceive(IAsyncResult result)
    {
        try
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedBytes = udpClient.EndReceive(result, ref remoteEndPoint);
            string receivedMessage = Encoding.UTF8.GetString(receivedBytes);
            string[] parts = receivedMessage.Split(':');
            string receivedIP = parts[0];
            string role = parts[1];

            if (receivedIP != localIP)
            {
                if (role == "HOST")
                {
                    currentHostIP = receivedIP;
                    lastHostPingTime = Time.time;
                    Debug.Log($"Host detected at {currentHostIP}");
                }
                else if (isHost && String.Compare(receivedIP, localIP) < 0)
                {
                    Debug.Log($"New client with lower IP ({receivedIP}) joined. Re-negotiating...");
                    HandOverHostRole(receivedIP);
                }
            }

            udpClient.BeginReceive(OnUdpReceive, null);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"UDP Receive Error: {e.Message}");
        }
    }

    void InitialRoleDecision()
    {
        if (currentHostIP == null)
        {
            BecomeHost();
        }
        else
        {
            BecomeClient(currentHostIP);
        }
    }

    void Update()
    {
        if (!isHost && currentHostIP != null && Time.time - lastHostPingTime > hostTimeout)
        {
            Debug.Log("Host timed out. Re-negotiating...");
            currentHostIP = null;
            networkManager.StopClient();
            Invoke(nameof(ReNegotiateRole), 1f);
        }
    }

    void ReNegotiateRole()
    {
        if (currentHostIP == null)
        {
            BecomeHost();
        }
        else
        {
            BecomeClient(currentHostIP);
        }
    }

    void BecomeHost()
    {
        if (!isHost)
        {
            Debug.Log("Becoming Host.");
            networkManager.StopClient();
            networkManager.StartHost();
            isHost = true;
            currentHostIP = localIP;
        }
    }

    void BecomeClient(string hostIP)
    {
        if (isHost)
        {
            Debug.Log("Switching from Host to Client.");
            networkManager.StopHost();
            isHost = false;
        }
        Debug.Log($"Connecting to host at {hostIP}");
        networkManager.networkAddress = hostIP;
        networkManager.StartClient();
    }

    void HandOverHostRole(string newHostIP)
    {
        if (isHost)
        {
            Debug.Log($"Handing over host role to {newHostIP}");
            networkManager.StopHost();
            isHost = false;
            if (!string.IsNullOrEmpty(newHostIP))
            {
                StartCoroutine(BecomeClientAfterDelay(newHostIP, 1f));
            }
            else
            {
                Debug.LogError("Cannot hand over to a null or empty IP!");
            }
        }
    }

    private System.Collections.IEnumerator BecomeClientAfterDelay(string hostIP, float delay)
    {
        yield return new WaitForSeconds(delay);
        BecomeClient(hostIP);
    }

    public void OnClientDisconnectHandler()
    {
        if (!isHost)
        {
            Debug.Log("Disconnected from host. Re-negotiating...");
            currentHostIP = null;
            Invoke(nameof(ReNegotiateRole), 1f);
        }
    }

    void OnDestroy()
    {
        CancelInvoke();
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}