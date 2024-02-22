using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class AutoDestroy : NetworkBehaviour
{
    public float delayBrforeDestroy= 2f;
    public ParticleSystem ps;
    public void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    public void Update() {

        if(!IsOwner) return;

        if(ps && !ps.IsAlive())
        {   
            Debug.Log("Destroy Obj");
            DestroyObject();
        }
    }   
    void DestroyObject()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject, delayBrforeDestroy);
    }
}
