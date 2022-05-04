using Project.PlayerM;
using Project.Utility.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Networking
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkRotation : MonoBehaviour
    {
        [SerializeField]
        [GreyOut]
        private float m_OldGunRotation;
        [SerializeField]
        [GreyOut]
        private float m_OldPlayerRotation;

        [SerializeField]
        public PlayerManager m_PlayerRot;

        private NetworkIdentity m_NetworkIdentity;
        private Rotation m_Player;     
        private float m_StillCounter = 0;

        private void Start()
        {
            m_NetworkIdentity = GetComponent<NetworkIdentity>();
            m_OldGunRotation = m_PlayerRot.GetLastRotation();

            m_Player = new Rotation();
            m_Player.gunRotation = 0;
            m_Player.playerRotation = 0;

            if (!m_NetworkIdentity.IsControlling())
            {
                enabled = false;
            }
        }

        private void Update()
        {
            if (m_NetworkIdentity.IsControlling())
            {
                if (m_OldPlayerRotation!=transform.localEulerAngles.y || m_OldGunRotation != m_PlayerRot.GetLastRotation())
                {
                    m_OldPlayerRotation = transform.localEulerAngles.y;
                    m_OldGunRotation = m_PlayerRot.GetLastRotation();
                    m_StillCounter = 0;
                    SendDataRotation();
                }
                else
                {
                    m_StillCounter += Time.deltaTime;
                    if (m_StillCounter >= 1)
                    {
                        m_StillCounter = 0;
                        SendDataRotation();
                    }
                }
            }
        }

        private void SendDataRotation()
        {
            m_Player.playerRotation = transform.localEulerAngles.y;
            m_Player.gunRotation = m_PlayerRot.GetLastRotation();
            m_NetworkIdentity.GetSocket().Emit("updateRotation", new JSONObject(JsonUtility.ToJson(m_Player)));
        }
    }
}

