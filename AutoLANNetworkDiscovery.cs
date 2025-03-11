using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Mirror.Discovery;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Network/Network Discovery")]
public class AutoLANNetworkDiscovery : NetworkDiscoveryBase<ServerRequest, ServerResponse>
{
    public SustieNetworkDiscovery networkDiscovery;
    // server
    // message to be sent back to the client
    protected override ServerResponse ProcessRequest(ServerRequest request, IPEndPoint endpoint)
    {
        try
        {
            return new ServerResponse
            {
                serverId = ServerId,
                uri = transport.ServerUri()
            };
        }
        catch(NotImplementedException)
        {
            Debug.LogError($"Transport{transport} does not support network discovery.");
            throw;
        }
    }

    // client
    // Process the answer from a server.
    protected override void ProcessResponse(ServerResponse response, IPEndPoint endpoint)
    {
        response.EndPoint = endpoint;

        UriBuilder realUri = new UriBuilder(response.uri)
        {
            Host = response.EndPoint.Address.ToString()
        };
        response.uri = realUri.Uri;
        networkDiscovery.OnDiscoveredServer(response);
    }
}
