using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using System.Linq;
using System;
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
      new NetworkString { info = "Player" }
      , NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
   public NetworkVariable<NetworkString> playerNameB = new NetworkVariable<NetworkString>(
      new NetworkString { info = "Player" }
      , NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
   public NetworkVariable<bool> changeColorRed = new NetworkVariable<bool>(
   false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


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
      loginManagerScript = FindObjectOfType<LoginManager>();
      if (IsOwner)
      {
         if (loginManagerScript != null)
         {
            string name = loginManagerScript.userNameInputField.text;
            if (IsOwnedByServer) { playerNameA.Value = name; }
            else { playerNameB.Value = name; }
         }

      }
   }
   public bool isChangingColor = false;
   public GameObject eyeRight;
   public GameObject eyeLeft;
   private void Update()
   {
      Vector3 nameLabelPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 2.5f, 0));
      nameLable.text = gameObject.name;
      nameLable.transform.position = nameLabelPos;
      if (IsOwner)
      {
         posX.Value = (int)System.Math.Ceiling(transform.position.x);
         if (Input.GetKey(KeyCode.F) && !isChangingColor)
         {
            Debug.Log("Change Color");
            if (changeColorRed.Value == false)
            {
               changeColorRed.Value = true;
               eyeRight.GetComponent<Renderer>().material = loginManagerScript.materialEyeRed;
               eyeLeft.GetComponent<Renderer>().material = loginManagerScript.materialEyeRed;

            }
            else
            {
               changeColorRed.Value = false;
               eyeRight.GetComponent<Renderer>().material = loginManagerScript.materialEyeBase;
               eyeLeft.GetComponent<Renderer>().material = loginManagerScript.materialEyeBase;


            }
            isChangingColor = true;
            StartCoroutine(delatTime());
         }
         if (Input.GetKeyDown(KeyCode.K))
         {
            TestServerRpc("Hello", new ServerRpcParams());
         }
         if (Input.GetKeyDown(KeyCode.L))
         {
            ClientRpcSendParams clientRpcSendParams = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } };
            ClientRpcParams clientRpcParams = new ClientRpcParams { Send = clientRpcSendParams };
            TestClientRpc("Hi, this is server ", clientRpcParams);
         }

      }

      UpdatePlayerinfo();
      UpdateMaterial();
   }
   [ClientRpc]
   private void TestClientRpc(string msg, ClientRpcParams clientRpcParams)
   {
      Debug.Log("Msg from server = " + msg);
   }
   [ServerRpc]
   private void TestServerRpc(string msg, ServerRpcParams serverRpcParams)
   {
      Debug.Log("Test server rpc from client = " + OwnerClientId);
   }
   IEnumerator delatTime()
   {
      yield return new WaitForSeconds(2.0f);
      isChangingColor = false;
   }
   private void UpdateMaterial()
   {
      if (loginManagerScript == null)
      {
         Debug.LogError("Login Manager Script is not assigned!");
         return;
      }

      Material eyeMaterial = changeColorRed.Value ? loginManagerScript.materialEyeRed : loginManagerScript.materialEyeBase;

      if (IsOwnedByServer)
      {
         eyeRight.GetComponent<Renderer>().material = eyeMaterial;
         eyeLeft.GetComponent<Renderer>().material = eyeMaterial;
         // Debug.Log("Host : " + changeColorRed.Value);
      }
      else
      {
         if (!IsLocalPlayer)
         {
            eyeRight.GetComponent<Renderer>().material = eyeMaterial;
            eyeLeft.GetComponent<Renderer>().material = eyeMaterial;
         }
         // Debug.Log("Client : " + changeColorRed.Value);
      }
   }


   private void UpdatePlayerinfo()
   {
      if (IsOwnedByServer) { nameLable.text = playerNameA.Value.ToString(); }
      else { nameLable.text = playerNameB.Value.ToString(); }
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
   private void OnEnable()
   {
      if(nameLable != null)
         nameLable.enabled = true;
   }
   private void OnDisable() {
      if(nameLable != null)
         nameLable.enabled = false;
   }
}
