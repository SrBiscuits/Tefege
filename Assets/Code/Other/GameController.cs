using Project.Managers;
using Project.Networking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Managers
{
    public class GameController : MonoBehaviour
    {
        static GameController m_Controller;
        public bool m_Host;
        Transform m_SpawnerZombie;
        public List<Transform> m_PlayerTransforms;
        List<Transform> m_ZombieSpawnTransform;
        public List<string> m_PlayerIDS;
        private NetworkIdentity m_SocketComponent;
        public GameObject m_DeadCamera;
        public GameLobby m_GameLobby;
        int m_Round = 1;
        float m_Instakill = 1;
        public float m_InstakillTimer = 30f;
        float m_CurrentInstakillTimer;
        public AudioSource m_SoundAudioSource;
        public AudioSource m_RoundAudioSource;
        public AudioClip m_MusicClip;
        public AudioClip m_RoundClip;

        public static GameController GetGameController()
        {
            return m_Controller;
        }
        void Awake()
        {
            if (m_Controller == null)
                m_Controller = this;
            else
                Destroy(m_Controller.gameObject);
        }
        private void Start()
        {
            if (!IsPlatformPc())
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            }
            m_SoundAudioSource.clip = m_MusicClip;
            m_SoundAudioSource.Play();
        }
        private void Update()
        {
            m_CurrentInstakillTimer -= Time.deltaTime;
            if (m_CurrentInstakillTimer <= 0)
                m_Instakill = 1;
        }
        public void AddOrRemovePlayer(Transform Player, bool Add)
        {
            if (Add)
                m_PlayerTransforms.Add(Player);
            else
                m_PlayerTransforms.Remove(Player);
        }
        public void AddOrRemovePlayerID(string Id, bool Add)
        {
            if (Add)
                m_PlayerIDS.Add(Id);
            else
                m_PlayerIDS.Remove(Id);
        }
        public List<Transform> GetPlayers()
        {
            return m_PlayerTransforms;
        }
        public List<string> GetPlayerIDS()
        {
            return m_PlayerIDS;
        }
        public void SetHost(bool Host)
        {
            m_Host = Host;
            if (m_Host && !IsPlatformPc())
                SendHostSwap();
            Debug.Log("host " + m_Host);
        }
        public bool GetHost()
        {
            return m_Host;
        }
        public void SetCloseSpawner(Transform Spawn)
        {
            m_SpawnerZombie = Spawn;
        }
        public Transform GetSpawn()
        {
            return m_SpawnerZombie;
        }
        public void SetAllZombieSpawnTransform(List<Transform> List)
        {
            m_ZombieSpawnTransform = List;
        }
        public List<Transform> GetAllZombieSpawner()
        {
            return m_ZombieSpawnTransform;
        }
        public void SetSocket(NetworkIdentity Socket)
        {
            m_SocketComponent = Socket;
        }
        public NetworkIdentity GetSocket()
        {
            return m_SocketComponent;
        }
        public GameObject GetCamera()
        {
            return m_DeadCamera;
        }
        public void NextRound()
        {
            m_Round++;
            m_RoundAudioSource.clip = m_RoundClip;
            m_RoundAudioSource.Play();
        }
        public int GetRound()
        {
            return m_Round;
        }
        public bool IsPlatformPc()
        {
#if UNITY_STANDALONE_WIN
            return true;
#elif UNITY_ANDROID || UNITY_IOS
        return false;
#endif
        }
        public void SendHostSwap()
        {
            m_SocketComponent.GetSocket().Emit("swapHost", new JSONObject(JsonUtility.ToJson(new LobbyID()
            {
                id = m_GameLobby.GetLobbyID()
            })));
        }
        public void SetInstaKill()
        {
            m_Instakill = 42069;
            m_CurrentInstakillTimer = m_InstakillTimer;
        }
        public float GetInstaKIll()
        {
            return m_Instakill;
        }
        public void CanStartGame()
        {
            m_GameLobby.CanStartGame();
        }
        public void StartGame()
        {
            m_DeadCamera.gameObject.SetActive(false);
        }
        public void ExitGame()
        {
            m_GameLobby.ExitEvent();
        }
    }
}

