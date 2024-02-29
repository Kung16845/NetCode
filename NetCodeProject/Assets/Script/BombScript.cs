using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class BombScript : NetworkBehaviour
{
   public BombSpawnerScript bombSpawnerScript;
   public GameObject effectPrefab;
   private void OnCollisionEnter(Collision collision) 
   {
        if(!IsOwner) return;

        if(collision.gameObject.tag == "Player")
        {
            ulong networkObjid = GetComponent<NetworkObject>().NetworkObjectId;
            SpawnEffect();
            
            bombSpawnerScript.DestroyServerRpc(networkObjid);
        }
   }
   private void SpawnEffect()
   {
        GameObject effect = Instantiate(effectPrefab,transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
   }
}
