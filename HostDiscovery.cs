// HostDiscovery.cs
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;

public class HostDiscovery : MonoBehaviour
{
    private int discoveryPort = 7778; // Different from game port
    private UdpClient udpClient;
    private bool isRunning = false;
    private string discoveredHost = null;

    public void SetDiscoveryPort(int port)
    {
        discoveryPort = port;
    }

    public async Task<string> DiscoverHost(int timeoutMs = 2000)
    {
        if (!isRunning)
        {
            await StartDiscovery();
        }

        var timeoutTask = Task.Delay(timeoutMs);
        while (discoveredHost == null && !timeoutTask.IsCompleted)
        {
            await Task.Yield();
        }

        return discoveredHost;
    }

    private async Task StartDiscovery()
    {
        try
        {
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            isRunning = true;

            // Send discovery message
            byte[] discoveryMessage = Encoding.UTF8.GetBytes("MIRROR_HOST_DISCOVERY");
            await udpClient.SendAsync(discoveryMessage, discoveryMessage.Length,
                new IPEndPoint(IPAddress.Broadcast, discoveryPort));

            // Start listening for responses
            ListenForResponses();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Discovery error: {e.Message}");
            isRunning = false;
        }
    }

    private async void ListenForResponses()
    {
        while (isRunning)
        {
            try
            {
                var result = await udpClient.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);
                
                if (message == "MIRROR_HOST_RESPONSE")
                {
                    discoveredHost = result.RemoteEndPoint.Address.ToString();
                    break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Receive error: {e.Message}");
                break;
            }
        }
        Cleanup();
    }

    private void Cleanup()
    {
        isRunning = false;
        udpClient?.Close();
        udpClient?.Dispose();
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}