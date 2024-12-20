using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFSW;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using QFSW.QC;
using UnityEngine.Assertions.Must;
using System.Security.Cryptography;
public class LobbyScript : MonoBehaviour
{
    Lobby hostLobby;
    string playerName;
    private void Start()
    {
        playerName = "MyName " + Random.Range(1, 9999);
        Debug.Log("Player name = " + playerName);
    }
    [Command]
    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "My lobby";
            int maxPlayers = 4;
            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                IsPrivate = false,
                Player = new Player()
                {
                    Data = new Dictionary<string, PlayerDataObject>{
                        {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
                    }
                },
                Data = new Dictionary<string, DataObject>{
                    {"GameMode",new DataObject(DataObject.VisibilityOptions.Public,"DeathMatch")}
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            hostLobby = lobby;
            PrintPlayers(hostLobby);
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
            Debug.Log("Create Lobby " + lobby.Name + "," + lobby.MaxPlayers + " , " + lobby.Id + " , " + lobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    [Command]
    private async void ListLobby()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter>()
            {
            new QueryFilter(
                field: QueryFilter.FieldOptions.AvailableSlots,
                op: QueryFilter.OpOptions.GT,
                value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder>()
            {
            new QueryOrder(
                asc: false,
                field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log("Lobby Found : " + lobbies.Results.Count);
            foreach (Lobby lobby in lobbies.Results)
            {
                Debug.Log(lobby.Name + " , " + lobby.MaxPlayers + " , " + lobby.Data["GameMode"].Value);
            }

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
    [Command]
    private async void JoinLobby(string lobbyCode)
    {
        try
        {
            // await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            // Debug.Log(" Joined by lobby code : " + lobbyCode);
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions{
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>{
                        {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
                    }
                }
            };
            Lobby joinLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode,options);
            Debug.Log("Join by lobby code : " + lobbyCode);
            PrintPlayers(joinLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    [Command]
    private async void QuickJoinLobby()
    {
        try
        {
            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            Debug.Log(lobby.Name + " , " + lobby.AvailableSlots);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    [Command]
    public void PrintPlayers(Lobby lobby)
    {   
        Debug.Log("Lobby : " + lobby.Name + " / " + lobby.Data["JoinCodeKey"].Value);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " : " + player.Data["PlayerName"].Value);
        }
    }
}
