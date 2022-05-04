using Project.Utility.Attributes;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Project.Networking
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkTransform : MonoBehaviour
    {
        [SerializeField]
        [GreyOut]
        private Vector3 m_OldPosition;

        private NetworkIdentity m_NetworkIdentity;
        private Player m_Player;
        private float m_StillCounter = 0;

        void Start()
        {
           // CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            m_NetworkIdentity = GetComponent<NetworkIdentity>();
            m_OldPosition = transform.position;
            m_Player = new Player();
            m_Player.position = new Position();
            m_Player.position.x = 0;
            m_Player.position.y = 0;
            m_Player.position.z = 0;

            if (!m_NetworkIdentity.IsControlling())
            {
                enabled = false;
            }
        }
        private void Update()
        {
            if (m_NetworkIdentity.IsControlling())
            {
                if (m_OldPosition != transform.position)
                {
                    m_OldPosition = transform.position;
                    
                    m_StillCounter = 0;
                    SendDataPosition();
                }
                else
                {
                    m_StillCounter += Time.deltaTime;
                    if (m_StillCounter >= 1)
                    {
                        m_StillCounter = 0;
                        SendDataPosition();
                    }
                }                
            } 
        }
        private void SendDataPosition()
        {
            m_Player.position.x = Mathf.Round(transform.position.x * 1000.0f) / 1000.0f;
            m_Player.position.y = Mathf.Round(transform.position.y * 1000.0f) / 1000.0f;
            m_Player.position.z = Mathf.Round(transform.position.z * 1000.0f) / 1000.0f;
            m_Player.id = m_NetworkIdentity.GetID();
            m_NetworkIdentity.GetSocket().Emit("updatePosition",new JSONObject(JsonUtility.ToJson(m_Player)));
        }
    }
}
