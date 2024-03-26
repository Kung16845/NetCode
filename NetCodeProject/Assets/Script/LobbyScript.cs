using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFSW;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using QFSW.QC;
using UnityEngine.Assertions.Must;
public class LobbyScript : MonoBehaviour
{
    Lobby hostLobby;
    [Command]
    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "My lobby";
            int maxPlayers = 4;
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            hostLobby = lobby;
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
            Debug.Log("Create Lobby " + lobby.Name + "," + lobby.MaxPlayers +" , " + lobby.Id + " , " + lobby.LobbyCode);
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
                Debug.Log(lobby.Name + " , " + lobby.MaxPlayers);
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
            await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            Debug.Log(" Joined by lobby code : " + lobbyCode);
            
                  
        }catch (LobbyServiceException e)
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
}
