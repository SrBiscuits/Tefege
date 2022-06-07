using Project.Managers;
using Project.Utility;
using SocketIO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.PlayerM
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject m_Container;
        public Button m_MobilePause;
        bool m_Paused;
        bool m_AbleToPause;
        private void Start()
        {
            m_Container.SetActive(false);
            m_MobilePause.gameObject.SetActive(false);
        }
        private void Update()
        {
            if (GameController.GetGameController().IsPlatformPc())
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    Pause();
            }
        }
        public void Disable()
        {
            m_Container.SetActive(false);
            m_AbleToPause = false;
        }
        public void Enable()
        {
            m_AbleToPause = true;
            if (!GameController.GetGameController().IsPlatformPc())
                m_MobilePause.gameObject.SetActive(true);
        }
        public void Pause()
        {
            if (m_AbleToPause)
            {
                if (m_Paused == false)
                {
                    m_Container.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    m_Container.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                }
                m_Paused = !m_Paused;
            }
        }
        public void ExitApplication()
        {
            SocketIOComponent l_Socket = GameController.GetGameController().GetSocket().GetSocket();
            l_Socket.Emit("disconnect");
            Application.Quit();
        }
        public void ExitGame()
        {
            SocketIOComponent l_Socket = GameController.GetGameController().GetSocket().GetSocket();
            l_Socket.Emit("disconnect");
            GameController.GetGameController().ExitGame();
            SceneMManager.Instance.UnLoadLevel(SceneList.LEVEL);
            SceneMManager.Instance.UnLoadLevel(SceneList.ONLINE);
            SceneManager.LoadScene("Intro", LoadSceneMode.Single);
        }
    }
}

