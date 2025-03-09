using Mirror;
using UnityEngine;

namespace Mirror
{
    public class CustomNetworkManager1 : NetworkManager
    {
        public DynamicIPRoleNegotiation roleNegotiator;

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect(); // Call base implementation
            if (roleNegotiator != null && !roleNegotiator.IsHost)
            {
                Debug.Log("Disconnected from host (via CustomNetworkManager). Re-negotiating...");
                roleNegotiator.OnClientDisconnectHandler();
            }
        }
    }
}