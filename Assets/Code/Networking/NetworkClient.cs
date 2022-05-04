using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using Project.Utility;
using System;
using Project.PlayerM;
using Project.Scriptable;
using Project.Gameplay;
using Project.AI;
using UnityEngine.Events;

namespace Project.Networking
{
    public class NetworkClient : SocketIOComponent
    {
        public const float SERVER_UPDATE_TIME = 10;

        public static Action<SocketIOEvent> m_OnStateGameChange=(E)=>{};
        public static Action<SocketIOEvent> m_OnAmmoPick = (E) => { };

        [Header("Network Client")]
        [SerializeField]
        private Transform m_NetworkContainer;
        [SerializeField]
        public GameObject m_PlayerPrefab;
        [SerializeField]
        private ServerObjects m_ServerSpawnObjects;
        [SerializeField]
        public GameObject m_OtherPlayerPrefab;
        [SerializeField]
        public GameObject m_PlayerIDPrefab;
        [SerializeField]
        private Transform m_PlayerIDPrefabContainer;

        List<PlayerManager> m_Players = new List<PlayerManager>();

        //ws://127.0.0.1:52300/socket.io/?EIO=3&transport=websocket
        //ws://tfgzombieswebsocket.herokuapp.com:80/socket.io/?EIO=3&transport=websocket

        //La puedes coger de donde quieras pero solo se cambia desde aquí
        public static string ClientID { get; private set; }

        private Dictionary<string, NetworkIdentity> m_ServerObjects;

        public override void Start() {
            //CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            base.Start();
            Initialize();
            SetupEvents();
        }

        // Update is called once per frame
        public override void Update() {
            base.Update();
        }

        private void Initialize()
        {
            m_ServerObjects = new Dictionary<string, NetworkIdentity>();
        }

        //Eventos llamados por server.js
        private void SetupEvents()
        {
            On("open", (E) =>
            {
                Debug.Log("connection made unity");
            });
            On("register", (E) =>
            {
                ClientID = E.data["id"].ToString().RemoveQuotes();
                Debug.LogFormat("Our Client's ID ({0})", ClientID);

                GameObject l_Client = Instantiate(m_PlayerIDPrefab, m_PlayerIDPrefabContainer);
                m_Players.Add(l_Client.GetComponent<PlayerManager>());
                l_Client.name = string.Format("Player ({0})", ClientID);
                NetworkIdentity l_NetworkIdentity = l_Client.GetComponent<NetworkIdentity>();
                l_NetworkIdentity.SetControllerID(ClientID);
                l_NetworkIdentity.SetSocketReference(this);
                GameController.GetGameController().SetSocket(l_NetworkIdentity);
            });
            On("spawn", (E) =>
            {
                string l_Id = E.data["id"].ToString().RemoveQuotes();

                GameObject l_Client = Instantiate(m_PlayerPrefab, m_NetworkContainer);
                GameController.GetGameController().AddOrRemovePlayer(l_Client.transform, true);
                GameController.GetGameController().AddOrRemovePlayerID(l_Id, true);
                l_Client.name = string.Format("Player ({0})", l_Id);
                NetworkIdentity l_NetworkIdentity = l_Client.GetComponent<NetworkIdentity>();
                l_NetworkIdentity.SetControllerID(l_Id);
                l_NetworkIdentity.SetSocketReference(this);
                m_ServerObjects.Add(l_Id, l_NetworkIdentity);
            });
            On("Otherspawn", (E) =>
            {
                string l_Id = E.data["id"].ToString().RemoveQuotes();

                GameObject l_Client = Instantiate(m_OtherPlayerPrefab, m_NetworkContainer);
                l_Client.name = string.Format("Player ({0})", l_Id);
                GameController.GetGameController().AddOrRemovePlayer(l_Client.transform, true);
                GameController.GetGameController().AddOrRemovePlayerID(l_Id, true);
                NetworkIdentity l_NetworkIdentity = l_Client.GetComponent<NetworkIdentity>();
                l_NetworkIdentity.SetControllerID(l_Id);
                l_NetworkIdentity.SetSocketReference(this);
                m_ServerObjects.Add(l_Id, l_NetworkIdentity);
            });
            On("disconnected", (E) =>
            {
                string l_Id = E.data["id"].ToString().RemoveQuotes();
                GameObject l_Client = m_ServerObjects[l_Id].gameObject;
                GameController.GetGameController().AddOrRemovePlayer(l_Client.transform, false);
                GameController.GetGameController().AddOrRemovePlayerID(l_Id, false);

                Destroy(l_Client);
                m_ServerObjects.Remove(l_Id);
            });
            On("host", (E) =>
            {
                GameController.GetGameController().SetHost();
            });
            On("updatePosition", (E) =>
            {            
                string l_Id = E.data["id"].ToString().RemoveQuotes();
                float x = float.Parse(E.data["position"]["x"].str);
                float y = float.Parse(E.data["position"]["y"].str);
                float z = float.Parse(E.data["position"]["z"].str);
                NetworkIdentity l_NetworkIdentity = m_ServerObjects[l_Id];
                if (l_NetworkIdentity.GetComponent<ZombieManager>() == null)
                    l_NetworkIdentity.transform.position = new Vector3(x, y, z);
                else 
                {
                    if(!GameController.GetGameController().GetHost())
                        l_NetworkIdentity.transform.position = new Vector3(x, y, z);
                }
            });
            On("updateBulletPosition", (E) =>
            {
                string l_Id = E.data["id"].ToString().RemoveQuotes();
                float x = E.data["position"]["x"].f;
                float y = E.data["position"]["y"].f;
                float z = E.data["position"]["z"].f;
                NetworkIdentity l_NetworkIdentity = m_ServerObjects[l_Id];
                l_NetworkIdentity.transform.position = new Vector3(x, y, z);
            });
            On("updateRotation", (E) =>
            {
                string l_Id = E.data["id"].ToString().RemoveQuotes();
                float l_GunRotation = E.data["gunRotation"].f;
                float l_PlayerRotation = E.data["playerRotation"].f;
                NetworkIdentity l_NetworkIdentity = m_ServerObjects[l_Id];
                l_NetworkIdentity.transform.localEulerAngles = new Vector3(0, l_PlayerRotation, 0);
                l_NetworkIdentity.GetComponent<PlayerManager>().SetRotation(l_GunRotation, l_PlayerRotation);
            });
            On("updateZombieRotation", (E) =>
            {
                string l_Id = E.data["id"].ToString().RemoveQuotes();
                float l_ZombieRotation = E.data["zombieRotation"].f;
                NetworkIdentity l_NetworkIdentity = m_ServerObjects[l_Id];
                if (!GameController.GetGameController().GetHost())
                    l_NetworkIdentity.GetComponent<ZombieManager>().SetZombieRotation(l_ZombieRotation);
            });
            On("serverSpawn", (E) =>
            {
                string name = E.data["name"].str;
                string l_Id = E.data["id"].ToString().RemoveQuotes();
                float x = E.data["position"]["x"].f;
                float y = 0;               
                if (E.data["name"].str == "MaxAmmo")
                    y = E.data["position"]["y"].f+0.3f;
                else
                    y = E.data["position"]["y"].f;
                float z = E.data["position"]["z"].f;
                if (!m_ServerObjects.ContainsKey(l_Id))
                {
                    ServerObjectsData l_ServerObjectData = m_ServerSpawnObjects.GetObjectByName(name);
                    var l_SpawnedObject = Instantiate(l_ServerObjectData.Prefab, m_NetworkContainer);

                    if (E.data["name"].str == "Zombie_AI")
                        l_SpawnedObject.transform.position = GameController.GetGameController().GetSpawn().position;
                    else
                        l_SpawnedObject.transform.position = new Vector3(x, y, z);
                    var l_NetworkIdentity = l_SpawnedObject.GetComponent<NetworkIdentity>();

                    l_NetworkIdentity.SetControllerID(l_Id);
                    l_NetworkIdentity.SetSocketReference(this);

                    //If bullet apply direction as well
                    if (name == "Bullet")
                    {
                        float directionX = E.data["direction"]["x"].f;
                        float directionY = E.data["direction"]["y"].f;
                        float directionZ = E.data["direction"]["z"].f;
                        string activator = E.data["activator"].ToString().RemoveQuotes();
                        float speed = E.data["speed"].f;
                        Vector3 l_CurrentRotation = new Vector3(directionX, directionY, directionZ);
                        l_SpawnedObject.transform.rotation = Quaternion.Euler(l_CurrentRotation);

                        WhoActivatedMe l_WhoActivatedMe = l_SpawnedObject.GetComponent<WhoActivatedMe>();
                        l_WhoActivatedMe.SetActivator(activator);

                        Bullet projectile = l_SpawnedObject.GetComponent<Bullet>();
                        projectile.Direction = new Vector3(directionX, directionY, directionZ);
                        projectile.Speed = speed;
                    }

                    m_ServerObjects.Add(l_Id, l_NetworkIdentity);
                }
            });
            On("serverUnspawn", (E) =>
            {
                string l_Id = E.data["id"].ToString().RemoveQuotes();
                NetworkIdentity l_NetworkIdentity = m_ServerObjects[l_Id];
                m_ServerObjects.Remove(l_Id);
                DestroyImmediate(l_NetworkIdentity.gameObject);
            });
            On("playerDied", (E) => {
                string l_Id = E.data["id"].ToString().RemoveQuotes();
                NetworkIdentity l_NetworkIdentity = m_ServerObjects[l_Id];
                if (E.data["username"].str!=null && E.data["username"].str == "Zombie_AI")
                {
                    ZombieManager l_Zombie = l_NetworkIdentity.gameObject.GetComponent<ZombieManager>();
                    if (GameController.GetGameController().GetHost())
                        l_Zombie.SetDieState();
                    else
                        l_Zombie.Die();
                }
                else
                {
                    l_NetworkIdentity.gameObject.SetActive(false);
                }              
            });
            On("playerRespawn", (E) => {
                Debug.Log("respawn");
                string l_Id = E.data["id"].ToString().RemoveQuotes();
                //float x = float.Parse(E.data["position"]["x"].str);
                //float y = float.Parse(E.data["position"]["y"].str);
                //float z = float.Parse(E.data["position"]["z"].str);
                NetworkIdentity l_NetworkIdentity = m_ServerObjects[l_Id];
                //l_NetworkIdentity.transform.position = new Vector3(x, y, z);
                ZombieManager l_Zombie = l_NetworkIdentity.gameObject.GetComponent<ZombieManager>();
                l_Zombie.gameObject.SetActive(true);
                l_Zombie.Respawn();
            });
            On("itemUsed", (E) =>{
                string l_Id = E.data["id"].ToString().RemoveQuotes();
                NetworkIdentity l_NetworkIdentity = m_ServerObjects[l_Id];
                Debug.Log("using item: " + E.data["name"].str);
                Debug.Log(E.data["name"].str == "MaxAmmo");
                if (E.data["name"].str == "MaxAmmo")
                {
                    
                    m_OnAmmoPick.Invoke(E);
                }
                m_ServerObjects.Remove(l_Id);
                DestroyImmediate(l_NetworkIdentity.gameObject);
            });
            On("loadGame", (E) =>
            {
                SceneMManager.Instance.LoadLevel(SceneList.LEVEL, (l_LevelName) =>
                {
                    SceneMManager.Instance.UnLoadLevel(SceneList.MAIN_MENU);
                });
            });
            On("lobbyUpdate", (E) =>
            {
                m_OnStateGameChange.Invoke(E);
            });
        }
        public void AttemptToJoinLobby()
        {
            Emit("joinGame");
        }
    }
    [Serializable]
    public class Player
    {
        public string id;
        public Position position;
        public Rotation rotation;
    }
    [Serializable]
    public class PlayerID
    {
        public string id;
    }
    [Serializable]
    public class Position
    {
        public float x;
        public float y;
        public float z;
    }
    [Serializable]
    public class Rotation
    {
        public float playerRotation;
        public float gunRotation;
    }
    [Serializable]
    public class Zombie
    {
        public string id;
        public float zombieRotation;
    }
    [Serializable]
    public class BulletData
    {
        public string id;
        public string activator;
        public Position position;
        public Position direction;
    }
    [Serializable]
    public class IDData
    {
        public string id;
        public string zombieID;
        public float damage;
        public float x;
        public float y;
        public float z;
    }
    [Serializable]
    public class LobbyID
    {
        public string id;
    }
}
