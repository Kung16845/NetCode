using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using QFSW.QC;
using TMPro;
public class LoginManager : MonoBehaviour
{
    public TMP_InputField userNameInputField;
    public TMP_InputField passwordInputField;
    private bool isApproveConection = false;
    [Command("set-approve")]

    public GameObject loginPanel;
    public GameObject leavePanel;
    public void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        loginPanel.SetActive(true);
        leavePanel.SetActive(false);

    }
    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) { return; }
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    public void HandleClientStarted(ulong clientId)
    {
        Debug.Log("HandleClientStarted client Id = " + clientId);
    }
    private void HandleClientConnected(ulong clientId)
    {   
        Debug.Log("HandlClientconnected Client ID " + clientId);
        if(clientId == NetworkManager.Singleton.LocalClientId)
        {
            loginPanel.SetActive(false);
            leavePanel.SetActive(true);
        }
    }
    public void HandleClientDisconnect(ulong clientId)
    {
        Debug.Log("HandleClientDisconnect client Id = " + clientId);
        if(NetworkManager.Singleton.IsHost) {}
        else if (NetworkManager.Singleton.IsClient)
        {
            Leave();
        }
    }
    public void Leave()
    {
        // shutdown
        // show login panel 
        // hide leave button
        if(NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
        else if(NetworkManager.Singleton.IsClient)
        { 
            NetworkManager.Singleton.Shutdown();
        }
        loginPanel.SetActive(true);
        leavePanel.SetActive(false); 
    }
    public void HandleServerStarted()
    {
        Debug.Log("HandleServerStarted");
    }
    public bool SetIsApproveConnection()
    {
        isApproveConection = !isApproveConection;
        return isApproveConection;
    }
    public void Host()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartHost();
    }
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;

        int byteLength = connectionData.Length;
        bool isApproved = false;
        if (byteLength > 0)
        {
            string clientData = System.Text.Encoding.ASCII.GetString(connectionData, 0, byteLength);
            string hostData = userNameInputField.GetComponent<TMP_InputField>().text;
            string passwordData = passwordInputField.GetComponent<TMP_InputField>().text;

            isApproved = ApprovalConnection(clientData, hostData, passwordData);
        }
        // Your approval logic determines the following values
        response.Approved = isApproved; //
        response.CreatePlayerObject = true;

        // // The Prefab hash value of the NetworkPrefab, if null the default NetworkManager player Prefab is used
        // response.PlayerPrefabHash = null;

        // // Position to spawn the player object (if null it uses default of Vector3.zero)
        // response.Position = Vector3.zero;

        // // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        // response.Rotation = Quaternion.identity;

        // // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
        // // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
        // response.Reason = "Some reason for not approving the client";

        // // If additional approval steps are needed, set this to true until the additional steps are complete
        // // once it transitions from true to false the connection approval response will be processed.
        // response.Pending = false;
    }
    public void Client()
    {
        string userName = userNameInputField.GetComponent<TMP_InputField>().text;
        string password = passwordInputField.GetComponent<TMP_InputField>().text;

        string connectionData = userName + "," + password;

        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(connectionData);

        Debug.Log("passwordClient In Client = " + password);
        NetworkManager.Singleton.StartClient();
        Debug.Log("Start Client");
    }
    public bool ApprovalConnection(string clientData, string hostData, string passwordData)
    {
        string[] clientDataArray = clientData.Split(',');

        bool isApprove = System.String.Equals(clientDataArray[0], hostData.Trim()) ? false : true
        && System.String.Equals(passwordData.Trim(), clientDataArray[1]) ? true : false;
        Debug.Log("isApprove = " + isApprove);
        Debug.Log("clientData = " + clientDataArray[0]);
        Debug.Log("passwordData = " + passwordData);
        Debug.Log("passwordClient = " + clientDataArray[1]);

        Debug.Log("isApprove2 = " + isApprove);
        return isApprove;
    }
}
