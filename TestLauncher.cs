// TestLauncher.cs
using UnityEngine;

public class TestLauncher : MonoBehaviour
{
    [SerializeField] private NetworkManagerCustom networkManager;
    [SerializeField] private int networkPort = 7777;
    [SerializeField] private int discoveryPort = 7778;

    void Start()
    {
        if (Application.isEditor)
        {
            // Use ports from Inspector for Editor instance
            networkManager.GetComponent<HostDiscovery>().SetDiscoveryPort(discoveryPort);
        }
        else
        {
            // Parse command line args for built instances
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-networkPort" && i + 1 < args.Length)
                {
                    int.TryParse(args[i + 1], out networkPort);
                }
                if (args[i] == "-discoveryPort" && i + 1 < args.Length)
                {
                    int.TryParse(args[i + 1], out discoveryPort);
                }
            }
            networkManager.GetComponent<HostDiscovery>().SetDiscoveryPort(discoveryPort);
        }

        networkManager.StartWithHostDiscovery(networkPort);
    }
}