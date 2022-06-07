using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Sound
{
    public class PlayerSounds : MonoBehaviour
    {
        public AudioSource m_AudioSource;
        public AudioClip m_Kaboom;
        public AudioClip m_MaxAmmo;
        public AudioClip m_Heal;
        public AudioClip m_Instakill;

        public void Kaboom()
        {
            AudioSource.PlayClipAtPoint(m_Kaboom, this.transform.position);
        }
        public void Instakill()
        {
            AudioSource.PlayClipAtPoint(m_Instakill, this.transform.position);
        }
        public void Heal()
        {
            AudioSource.PlayClipAtPoint(m_Heal, this.transform.position);
        }
        public void MaxAmmo()
        {
            AudioSource.PlayClipAtPoint(m_MaxAmmo, this.transform.position);
        }
    }
}
