
// NetworkManagerCustom.cs
using Mirror;
using UnityEngine;
using System.Net;
using System.Threading.Tasks;
using kcp2k;
using System.Net.Sockets;
using System.Text;

public class NetworkManagerCustom : NetworkManager
{
    [SerializeField]
    private int discoveryPort = 7778;
    private HostDiscovery hostDiscovery;

    public override void Awake()
    {
        base.Awake();
        hostDiscovery = gameObject.AddComponent<HostDiscovery>();
        hostDiscovery.SetDiscoveryPort(discoveryPort);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        // Player is spawned here, but we’ll handle discovery separately
    }

    public async void StartWithHostDiscovery(int networkPort = 7777)
    {
        var transport = GetComponent<KcpTransport>();
        if(transport != null)
        {
            transport.port = (ushort)networkPort;
        }
        else
        {
            Debug.LogError("No Transport component found on NetworkManager!");
            return;
        }
        
        // Start as host initially
        StartHost();

        // Look for other hosts
        string discoveredHostIP = await hostDiscovery.DiscoverHost();
        
        // Wait for a player to be spawned before handling discovery
        if (discoveredHostIP != null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in players)
            {
                var switchManager = player.GetComponent<HostSwitchManager>();
                if (switchManager != null)
                {
                    switchManager.HandleHostDiscovery(discoveredHostIP, networkPort);
                    break; // Only need one local player to handle it
                }
            }
        }
    }

    // Add this to respond to discovery requests when hosting
    public override void OnStartServer()
    {
        base.OnStartServer();
        StartHostResponseListener();
    }

    private async void StartHostResponseListener()
    {
        using (var udpClient = new UdpClient(discoveryPort))
        {
            while (NetworkServer.active)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    string message = Encoding.UTF8.GetString(result.Buffer);

                    if (message == "MIRROR_HOST_DISCOVERY")
                    {
                        byte[] response = Encoding.UTF8.GetBytes("MIRROR_HOST_RESPONSE");
                        await udpClient.SendAsync(response, response.Length,
                            result.RemoteEndPoint);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Host response error: {e.Message}");
                    break;
                }
            }
        }
    }
}