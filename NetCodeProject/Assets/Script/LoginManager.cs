using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using QFSW.QC;
using TMPro;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField chractorIDInputField;
    public List<uint> AlternativePlayersPrefabs;
    public TMP_InputField userNameInputField;
    public TMP_InputField passwordInputField;
    private bool isApproveConection = false;
    [Command("set-approve")]
    public List<int> numposition = new List<int>() { 0, 1, 2, 3, };
    public GameObject loginPanel;
    public GameObject leavePanel;
    public string nameClient;
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
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            loginPanel.SetActive(false);
            leavePanel.SetActive(true);
        }
        CheckVariableInListnumposition();
    }
    public void CheckVariableInListnumposition()
    {
        foreach (int i in numposition)
        {
            Debug.Log(i);
        }
    }
    public void HandleClientDisconnect(ulong clientId)
    {
        Debug.Log("HandleClientDisconnect client Id = " + clientId);
        if (NetworkManager.Singleton.IsHost) { }
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
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
        else if (NetworkManager.Singleton.IsClient)
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
        var clientId = request.ClientNetworkId;
        var connectionData = request.Payload;
        int byteLength = connectionData.Length;
        bool isApproved = false;

        // Separate logic for host and client
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Host logic
            isApproved = true; // Host is always approved

            // Host can select the prefab based on the character ID input field
            int hostCharacterId;
            if (int.TryParse(chractorIDInputField.text, out hostCharacterId) && hostCharacterId >= 0 && hostCharacterId < AlternativePlayersPrefabs.Count)
            {
                response.PlayerPrefabHash = new Nullable<uint>(AlternativePlayersPrefabs[hostCharacterId]);
            }
            else
            {
                response.PlayerPrefabHash = new Nullable<uint>(AlternativePlayersPrefabs[0]); // Default to first prefab if input is invalid
            }

            
        }
        else
        {
            // Client logic
            if (byteLength > 0)
            {
                string clientData = System.Text.Encoding.ASCII.GetString(connectionData, 0, byteLength);
                string hostData = userNameInputField.text.Trim();
                string passwordData = passwordInputField.text.Trim();

                isApproved = ApprovalConnection(clientData, hostData, passwordData);
                string[] clientDatas = clientData.Split(",");
                nameClient = clientDatas[0];
                // userNameInputField.text = clientDatas[0]; 
                response.PlayerPrefabHash = new Nullable<uint>(AlternativePlayersPrefabs[int.Parse(clientDatas[2])]);
            }

            // Client cannot select prefab; it's assigned by the host
            // Default to first prefab
        }

        response.Approved = isApproved;
        response.CreatePlayerObject = isApproved;
        SetSpawnLocation(clientId, response);

        if (!isApproved)
        {
            response.Reason = "Invalid login credentials or character ID";
        }

        response.Pending = false;
    }
    public void SelectPrefab(string num)
    {
        chractorIDInputField.GetComponent<TMP_InputField>().text = num;
    }
    public void Client()
    {
        string userName = userNameInputField.GetComponent<TMP_InputField>().text;
        string password = passwordInputField.GetComponent<TMP_InputField>().text;
        string chractorID = chractorIDInputField.GetComponent<TMP_InputField>().text;
        // userNameInputField.text = userName; 
        // string[] inputFiels = {userName,password ,chractorID};
        // string clientData = HelperScript.CombineString(inputFiels);

        string connectionData = userName + "," + password + "," + chractorID;

        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(connectionData);

        Debug.Log("passwordClient In Client = " + password);
        NetworkManager.Singleton.StartClient();
        Debug.Log("Start Client");
    }
    public void SetSpawnLocation(ulong clientId
    , NetworkManager.ConnectionApprovalResponse response)
    {
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;
        //server 
        // if (clientId == NetworkManager.Singleton.LocalClientId)
        // {
        //     spawnPos = new Vector3(-2f, 0f, 0f);
        //     spawnRot = Quaternion.Euler(0f, 135f, 0f);
        // }
        // else
        // {
        int numRandom = UnityEngine.Random.Range(0, numposition.Count);
        int selectedNumber = numposition.ElementAt(numRandom);
        switch (selectedNumber)
        {
            case 0:
                spawnPos = new Vector3(-2f, 0f, 0f); spawnRot = Quaternion.Euler(0f, 135f, 0f);
                // numposition.Remove(0);
                Debug.Log(numposition);
                break;
            case 1:
                spawnPos = new Vector3(0f, 0f, 0f); spawnRot = Quaternion.Euler(0f, 180f, 0f);
                // numposition.Remove(1);
                break;
            case 2:
                spawnPos = new Vector3(2f, 0f, 0f); spawnRot = Quaternion.Euler(0f, 225f, 0f);
                // numposition.Remove(2);
                break;
            case 3:
                spawnPos = new Vector3(4f, 0f, 0f); spawnRot = Quaternion.Euler(0f, 270f, 0f);
                // numposition.Remove(3);
                break;
        }
        // }
        CheckVariableInListnumposition();
        response.Position = spawnPos;
        response.Rotation = spawnRot;
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
