using Project.Managers;
using Project.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.PlayerM
{
    public class PlayerCanvas : MonoBehaviour
    {
        public TextMeshProUGUI m_CurrentAmmoText;
        public TextMeshProUGUI m_ReservedAmmo;
        public TextMeshProUGUI m_RoundText;
        public Image m_BloodImage;
        float m_Health = 100;
        float m_NoDamageDealtCooldown;
        public Image m_Instakill;
        bool m_InstakillActive;

        private void Start()
        {
            m_Instakill.gameObject.SetActive(false);
        }
        public void SetCurrentAmmo(int Value)
        {
            if(Value<10)
                m_CurrentAmmoText.text = ("0"+Value.ToString());
            else
                m_CurrentAmmoText.text = Value.ToString();

            float l_RedAmount = 0;
            float l_Prct = 100;
            if (Value != 0)
            {
                l_Prct = 30 / Value;
                l_RedAmount = 255 / l_Prct;
            }
            byte l_ByteRed = Convert.ToByte(l_RedAmount);

            m_CurrentAmmoText.color = new Color32(255, l_ByteRed, l_ByteRed, 255);
        }
        public void SetReservedAmmo(int Value)
        {
            m_ReservedAmmo.text = ("/" + Value.ToString());
        }
        public void SetRound(string Round)
        {
            m_RoundText.text = Round;
        }
        public void DamageReceived()
        {
            if (m_Health > 0)
                m_Health -= 20;
            m_NoDamageDealtCooldown = 1.5f;
            m_BloodImage.color = new Color(1f, 1f, 1f, 1f - (m_Health / 100));
        }
        private void Update()
        {
            if (m_Health < 100.0f)
            {
                m_NoDamageDealtCooldown -= Time.deltaTime;
                if(m_NoDamageDealtCooldown<=0)
                {
                    m_Health += Time.deltaTime*5;
                    m_BloodImage.color = new Color(1f, 1f, 1f, 1f-(m_Health/100));
                }
            }

            if (m_InstakillActive)
            {
                if (GameController.GetGameController().GetInstaKIll() == 1)
                {
                    m_InstakillActive = false;
                    m_Instakill.gameObject.SetActive(false);
                }
            }
        }
        public void FullHealth()
        {
            m_Health = 100.0f;
            m_BloodImage.color = new Color(1f, 1f, 1f, 0f);
        }
        public void ActivateInstakill()
        {
            m_InstakillActive = true;
            m_Instakill.gameObject.SetActive(true);
        }
    }
}

