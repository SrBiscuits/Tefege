using Project.Networking;
using Project.PlayerM;
using Project.Utility;
using SocketIO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.Managers
{
    public class GameLobby : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_LobbyContainer;
        [SerializeField]
        private GameObject m_EndGameContainer;
        [SerializeField]
        private List<GameObject> m_Players;
        [SerializeField]
        private TextMeshProUGUI m_RoundText;
        string m_LobbyID;

        public Button m_StartGameButton;
        public Button m_BackButton;
        public PauseMenu m_PauseMenu;
        void Start()
        {
            NetworkClient.m_OnStateGameChange += OnGameStateChange;
            m_LobbyContainer.SetActive(false);
            m_StartGameButton.image.color = new Color(0.5f, 0.5f, 0.5f);
            m_BackButton.image.color = new Color(0.5f, 0.5f, 0.5f);
            m_StartGameButton.interactable = false;
            m_BackButton.interactable = false;
        }
        private void OnGameStateChange(SocketIOEvent e)
        {
            string state = e.data["state"].str;
            float players = e.data["players"].f;            
            float lobbyState = 0;
            switch (state)
            {
                case "Game":
                    m_LobbyContainer.SetActive(false);
                    m_PauseMenu.Enable();
                    break;
                case "EndGame":
                    StartCoroutine(EndGame());
                    m_PauseMenu.Disable();
                    lobbyState = 2;
                    break;
                case "Lobby":
                    m_LobbyContainer.SetActive(true);
                    lobbyState = 1;
                    break;                   
                default:
                    m_LobbyContainer.SetActive(false);
                    m_PauseMenu.Enable();
                    break;                
            }
            switch (players)
            {
                case 1:
                    if (lobbyState == 1)
                    {
                        m_Players[0].SetActive(true);
                        m_Players[1].SetActive(false);
                        m_Players[2].SetActive(false);
                        m_Players[3].SetActive(false);
                    }
                    else
                    {
                        m_Players[4].SetActive(true);
                        m_Players[5].SetActive(false);
                        m_Players[6].SetActive(false);
                        m_Players[7].SetActive(false);
                    }
                    break;
                case 2:
                    if (lobbyState == 1)
                    {
                        m_Players[1].SetActive(true);
                        m_Players[2].SetActive(false);
                        m_Players[3].SetActive(false);
                    }
                    else
                    {
                        m_Players[4].SetActive(true);
                        m_Players[5].SetActive(true);
                        m_Players[6].SetActive(false);
                        m_Players[7].SetActive(false);
                    }
                    break;
                case 3:
                    if (lobbyState == 1)
                    {
                        m_Players[2].SetActive(true);
                        m_Players[3].SetActive(false);
                    }
                    else
                    {
                        m_Players[4].SetActive(true);
                        m_Players[5].SetActive(true);
                        m_Players[6].SetActive(true);
                        m_Players[7].SetActive(false);
                    }
                    break;
                case 4:
                    if (lobbyState == 1)
                        m_Players[3].SetActive(true);
                    else
                    {
                        m_Players[4].SetActive(true);
                        m_Players[5].SetActive(true);
                        m_Players[6].SetActive(true);
                        m_Players[7].SetActive(true);
                    }
                    break;
                default:
                    m_Players[0].SetActive(true);
                    break;
            }
            m_LobbyID = e.data["id"].str;
        }
        public void CanStartGame()
        {
            m_StartGameButton.image.color = new Color(1f, 1f, 1f);
            m_StartGameButton.interactable = true;
            m_BackButton.image.color = new Color(1f, 1f, 1f);
            m_BackButton.interactable = true;
            Debug.Log("canstart");
        }
        public void StartGame()
        {
            SocketIOComponent l_Socket = GameController.GetGameController().GetSocket().GetSocket();
            l_Socket.Emit("startGame", new JSONObject(JsonUtility.ToJson(new LobbyID()
            {
                id = m_LobbyID
            })));
            //m_LobbyContainer.SetActive(false);           
        }
        public void ExitGame()
        {         
            SocketIOComponent l_Socket = GameController.GetGameController().GetSocket().GetSocket();
            l_Socket.Emit("exitGame", new JSONObject(JsonUtility.ToJson(new LobbyID()
            {
                id = m_LobbyID
            })));          
            //Application.Quit();
        }
        public void ExitApplication()
        {
            Application.Quit();
        }
        public void ExitEvent()
        {
            NetworkClient.m_OnStateGameChange -= OnGameStateChange;
        }
        public void Back2Menu()
        {
            SocketIOComponent l_Socket = GameController.GetGameController().GetSocket().GetSocket();
            l_Socket.Emit("disconnect");
            GameController.GetGameController().ExitGame();
            SceneMManager.Instance.UnLoadLevel(SceneList.LEVEL);
            SceneMManager.Instance.UnLoadLevel(SceneList.ONLINE);
            SceneManager.LoadScene("Intro", LoadSceneMode.Single);
        }
        private IEnumerator EndGame()
        {
            Debug.Log("Endgame");
            yield return new WaitForSeconds(2f);
            m_EndGameContainer.SetActive(true);
            m_RoundText.text = ("You survived: " + GameController.GetGameController().GetRound() + " rounds");
            Cursor.lockState = CursorLockMode.None;
        }
        public string GetLobbyID()
        {
            return m_LobbyID;
        }
    }
}

