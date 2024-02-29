using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
public class BombSpawnerScript : NetworkBehaviour
{
    public GameObject bombPrefab;
    public List<GameObject> spawnedBomb = new List<GameObject>();
    public OwnerNetworkAnimationScript ownerNetworkAnimationScript;
    void Start() {
        ownerNetworkAnimationScript = GetComponent<OwnerNetworkAnimationScript>();
    }
    void Update()
    {
        if (!IsOwner) return;
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            ownerNetworkAnimationScript.SetTrigger("Pickup");
            SpawnBompServerRpc();
        }

    }
    [ServerRpc]
    void SpawnBompServerRpc()
    {
        Vector3 spawnPos = transform.position + (transform.forward * -1.5f) + (transform.up * 3.5f);
        Quaternion spawnRot = transform.rotation;
        GameObject bomb = Instantiate(bombPrefab, spawnPos, spawnRot);
        spawnedBomb.Add(bomb);
        bomb.GetComponent<BombScript>().bombSpawnerScript = this;
        bomb.GetComponent<NetworkObject>().Spawn();

    }
    [ServerRpc (RequireOwnership = false)]  
    public void DestroyServerRpc(ulong networkObjectId)
    {
        GameObject obj = FindSpawnedBomb(networkObjectId);
        if(obj == null) return;
        obj.GetComponent<NetworkObject>().Despawn();
        spawnedBomb.Remove(obj); Destroy(obj);
    }
    private GameObject FindSpawnedBomb(ulong networkObjid)
    {
        foreach (GameObject bomb in spawnedBomb)
        {
            ulong bombId = bomb.GetComponent<NetworkObject>().NetworkObjectId;
            if(bombId == networkObjid)
            {
                return bomb;
            }
        }
        return null;
    }
}
