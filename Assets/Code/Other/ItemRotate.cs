using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Gameplay
{
    public class ItemRotate : MonoBehaviour
    {
        float m_Timer = 30.0f;
        float m_TickVelocity = 1f;
        public GameObject m_Item;
        bool m_Active = true;
        public float m_RotateVelocityX;
        public float m_RotateVelocityY;
        public float m_RotateVelocityZ;

        void Update()
        {
            m_Timer -= Time.deltaTime;
            transform.Rotate(new Vector3(m_RotateVelocityX, m_RotateVelocityY, m_RotateVelocityZ) * Time.deltaTime);
            if (m_Timer < 11)
            {
                m_TickVelocity -= Time.deltaTime;
                if (m_TickVelocity <= 0)
                {
                    if (m_Active)
                        m_Item.SetActive(false);
                    else
                        m_Item.SetActive(true);
                    m_Active = !m_Active;

                    if (m_Timer <= 10)
                        m_TickVelocity = 1f;
                    if (m_Timer <= 6)
                        m_TickVelocity = 0.5f;
                    if (m_Timer <= 3)
                        m_TickVelocity = 0.2f;
                }
            }
        }
    }
}

