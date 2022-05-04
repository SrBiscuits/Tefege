using Project.Networking;
using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLobby : MonoBehaviour
{
    [SerializeField]
    private GameObject m_LobbyContainer;
    [SerializeField]
    private List<GameObject> m_Players;

    string m_LobbyID;

    void Start()
    {
        NetworkClient.m_OnStateGameChange += OnGameStateChange;
        m_LobbyContainer.SetActive(false);
    }
    private void OnGameStateChange(SocketIOEvent e)
    {
        string state = e.data["state"].str;
        float players = e.data["players"].f;

        switch (state)
        {
            case "Game":
                m_LobbyContainer.SetActive(false);
                Debug.Log("s");
                break;
            case "EndGame":
                m_LobbyContainer.SetActive(false);
                break;
            case "Lobby":
                m_LobbyContainer.SetActive(true);
                break;
            default:
                m_LobbyContainer.SetActive(false);
                break;
        }

        switch (players)
        {
            case 1:
                m_Players[0].SetActive(true);
                m_Players[1].SetActive(false);
                m_Players[2].SetActive(false);
                m_Players[3].SetActive(false);
                break;
            case 2:
                m_Players[1].SetActive(true);
                m_Players[2].SetActive(false);
                m_Players[3].SetActive(false);
                break;
            case 3:
                m_Players[2].SetActive(true);
                m_Players[3].SetActive(false);
                break;
            case 4:
                m_Players[3].SetActive(true);
                break;
            default:
                m_Players[0].SetActive(true);
                break;
        }
        m_LobbyID= e.data["id"].str;
    }
    public void StartGame()
    {
        SocketIOComponent l_Socket = GameController.GetGameController().GetSocket().GetSocket();
        l_Socket.Emit("startGame", new JSONObject(JsonUtility.ToJson(new LobbyID()
        {
            id=m_LobbyID
        })));
    }
}
