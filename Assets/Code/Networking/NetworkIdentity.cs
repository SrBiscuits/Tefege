using Project.Utility.Attributes;
using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Networking
{
    public class NetworkIdentity : MonoBehaviour
    {
        [Header("Helpful values")]
        [SerializeField]
        [GreyOut]
        private string m_Id;
        [SerializeField]
        [GreyOut]
        private bool m_IsControlling;

        private SocketIOComponent m_Socket;

        void Awake()
        {
            m_IsControlling = false;
        }
        public void SetControllerID(string ID)
        {
            m_Id = ID;
            m_IsControlling = (NetworkClient.ClientID == ID) ? true : false;
        }
        public void SetSocketReference(SocketIOComponent Socket)
        {
            m_Socket = Socket;
        }
        public string GetID()
        {
            return m_Id;
        }
        public bool IsControlling()
        {
            return m_IsControlling;
        }
        public SocketIOComponent GetSocket()
        {
            return m_Socket;
        }
    }
}
