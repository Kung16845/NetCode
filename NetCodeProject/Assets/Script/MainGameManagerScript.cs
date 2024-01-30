using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class MainGameManagerScript : MonoBehaviour
{   
    public void OnSeverButton()
    {
        NetworkManager.Singleton.StartServer();
    }
    public void OnHostButton()
    {
        NetworkManager.Singleton.StartHost();
    }
    public void OnClientButton()
    {
        NetworkManager.Singleton.StartClient();
    }
}
