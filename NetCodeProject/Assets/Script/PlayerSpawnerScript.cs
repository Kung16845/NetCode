using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnerScript : NetworkBehaviour
{
    Movement movement;
    public Behaviour[] scripts;
    private Renderer[] renderers;
    // Start is called before the first frame update
    void Start()
    {
        movement = gameObject.GetComponent<Movement>();
        renderers = GetComponentsInChildren<Renderer>();
    }
    void SetPlayerStete(bool state)
    {
        foreach(var script in scripts) {script.enabled = state;}
        foreach(var renderers in renderers) {renderers.enabled = state;}

    }
    private Vector3 GetRandomPos()
    {
        Vector3 randPos = new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        return randPos;
    }
    public void Respawn()
    {
        RespawnServerRpc();
    }
    [ServerRpc]
    void RespawnServerRpc()
    {
        Vector3 pos = GetRandomPos();
        RespawnClientRpc(pos);
    }
    [ClientRpc]
    void RespawnClientRpc(Vector3 spanwPos)
    {
        // movement.enabled = false;
        // transform.position = spanwPos;
        // movement.enabled = true;
        StartCoroutine(RespwnCouritine(spanwPos));
    }
    IEnumerator RespwnCouritine(Vector3 spawnPos)
    {
        SetPlayerStete(false);
        transform.position = spawnPos;
        yield return new WaitForSeconds(3f);
        SetPlayerStete(true);
    }
}
