using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;
public class Movement : NetworkBehaviour
{
   public Rigidbody rb;
   public float speed = 0.5f;
   public float rotationspeed = 10.0f;
   public TMP_Text namePrefab;
   private TMP_Text nameLable;
   public string nameHost;
   private NetworkVariable<int> posX = new NetworkVariable<int>(
      0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
   public struct NetworkString : INetworkSerializable
   {
      public FixedString32Bytes info;
      public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
      {
         serializer.SerializeValue(ref info);
      }
      public override string ToString()
      {
         return info.ToString();
      }
      public static implicit operator NetworkString(string v) =>
      new NetworkString() { info = new FixedString32Bytes(v) };
   }

   public NetworkVariable<NetworkString> playerNameA = new NetworkVariable<NetworkString>(
      new NetworkString { info = "Player"}
      ,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner );
   public NetworkVariable<NetworkString> playerNameB = new NetworkVariable<NetworkString>(
      new NetworkString { info = "Player"}
      ,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner );
   
   public LoginManager loginManagerScript;
   public override void OnNetworkSpawn()
   {
      GameObject canvas = GameObject.FindWithTag("MainCanvas");
      nameLable = Instantiate(namePrefab, Vector3.zero, Quaternion.identity) as TMP_Text;
      nameLable.transform.SetParent(canvas.transform);
      posX.OnValueChanged += (int previousValue, int newValue) =>
      {
         Debug.Log("Owner ID = " + OwnerClientId + " : Pos X = " + posX.Value);
      };
      // if(IsServer)
      // {  
      //    var LoginManager = FindObjectOfType<LoginManager>();
      //    playerNameA.Value = new NetworkString() {info = new FixedString32Bytes(LoginManager.userNameInputField.text)};
         
      //    playerNameB.Value = new NetworkString() {info = new FixedString32Bytes(LoginManager.nameClient)};
      // }
      if(IsOwner)
      {
         loginManagerScript = GameObject.FindObjectOfType<LoginManager>();
         if(loginManagerScript != null)
         {
            string name = loginManagerScript.userNameInputField.text;
            if(IsOwnedByServer) {playerNameA.Value = name;}
            else {playerNameB.Value = name;}
         }
      }
   }
   private void Update()
   {
      Vector3 nameLabelPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 2.5f, 0));
      nameLable.text = gameObject.name;
      nameLable.transform.position = nameLabelPos;
      if (IsOwner)
      {
         posX.Value = (int)System.Math.Ceiling(transform.position.x);
      }
      UpdatePlayerinfo();
   }
   private void UpdatePlayerinfo()
   {
      if(IsOwnedByServer) {nameLable.text = playerNameA.Value.ToString();}
      else {nameLable.text = playerNameB.Value.ToString();}
   }

   public override void OnDestroy()
   {
      if (nameLable != null) Destroy(nameLable.gameObject);
      base.OnDestroy();
   }
   void Start()
   {
      rb = this.GetComponent<Rigidbody>();
   }
   private void FixedUpdate()
   {
      if (IsOwner)
      {
         float translation = Input.GetAxis("Vertical") * speed;
         float rotation = Input.GetAxis("Horizontal") * rotationspeed;
         translation *= Time.deltaTime;
         Quaternion turn = Quaternion.Euler(0f, rotation, 0f);
         rb.MovePosition(rb.position + this.transform.forward * translation);
         rb.MoveRotation(rb.rotation * turn);
      }
   }
}
