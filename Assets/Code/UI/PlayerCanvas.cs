using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Project.PlayerM
{
    public class PlayerCanvas : MonoBehaviour
    {
        public TextMeshProUGUI m_CurrentAmmoText;
        public TextMeshProUGUI m_ReservedAmmo;

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
    }
}

