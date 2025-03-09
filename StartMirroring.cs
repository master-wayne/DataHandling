using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMirroring : MonoBehaviour
{ 
    public NetworkManagerCustom networkManager;
    void Start()
    {
        networkManager.StartWithHostDiscovery();
        
    }


}
