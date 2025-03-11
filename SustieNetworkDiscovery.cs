using System.Collections;
using System.Collections.Generic;
using Mirror.Discovery;
using UnityEngine;

public class SustieNetworkDiscovery : MonoBehaviour
{
    public AutoLANNetworkDiscovery networkDiscovery;
    readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

    void Start()
    {
        StartCoroutine(AutoConnect()); 
    }

    public IEnumerator AutoConnect()
    {
        discoveredServers.Clear();
        networkDiscovery.StartDiscovery();
        
        Debug.Log("Looking for host...");

        yield return new WaitForSeconds(3.1f);

        if(discoveredServers == null || discoveredServers.Count <= 0)
        {
            Debug.Log("No host found. Starting one...");

            yield return new WaitForSeconds(1.0f);
            SustieNetworkManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();
        }
    }

    void Connect(ServerResponse info)
    {
        Debug.Log("Connecting to server : " + info.serverId);
        networkDiscovery.StopDiscovery();
        SustieNetworkManager.singleton.StartClient();        
    }
    public void OnDiscoveredServer(ServerResponse info)
    {
        discoveredServers[info.serverId] = info;
        Connect(info);
    }
}

/* public class HostManager : MonoBehaviour
{
    public float minDelay = 1f;
    public float maxDelay = 5f;

    void Start()
    {
        StartCoroutine(TryBecomeHost());
    }

    IEnumerator TryBecomeHost()
    {
        float delay = Random.Range(minDelay, maxDelay);
        yield return new WaitForSeconds(delay);

        if (!NetworkServer.active)
        {
            NetworkManager.singleton.StartHost();
        }
    }
}
 */