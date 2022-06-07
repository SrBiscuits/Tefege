using System.Collections;
using System.Collections.Generic;
using Project.Networking;
using Project.Utility;
using SocketIO;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField]
        private Button m_Button;
        private SocketIOComponent m_SocketReference;

        public SocketIOComponent SocketReference
        {
            get
            {
                return m_SocketReference = (m_SocketReference == null) ? FindObjectOfType<NetworkClient>() : m_SocketReference;
            }
        }
        void Start()
        {
            m_Button.interactable = false;
            SceneMManager.Instance.LoadLevel(SceneList.ONLINE, (l_Levelname) => {});          
            m_Button.interactable = true;
        }
        public void OnQueue()
        {
            SocketReference.Emit("joinGame");
        }
    }
}

