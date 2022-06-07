using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.PlayerM
{
    public class OtherPlayerAnimator : MonoBehaviour
    {
        Animator m_OtherPlayerAnimator;
        public GameObject m_MyGun;
        public GameObject m_MyAkMag;
        public GameObject m_Gun;
        public GameObject m_AkMag;
        bool m_Aim;
        Vector2 m_CurrentAnimationBlendVector;
        Vector2 m_AnimationVelocity;
        public float m_AnimationSmoothTime = 0.1f;
        public AudioSource m_AudioSource;
        public AudioClip m_ReloadAudio;
        public AudioClip m_ShootAudio;

        private void Awake()
        {
            m_OtherPlayerAnimator = GetComponent<Animator>();
        }
        public void DeadAnimation()
        {
            m_OtherPlayerAnimator.SetTrigger("Dead");
            Instantiate(m_Gun, m_MyGun.transform.position, Quaternion.identity);
            m_MyGun.gameObject.SetActive(false);
            StartCoroutine(DisablePlayer());
        }
        public void AkMag()
        {
            Instantiate(m_AkMag, m_MyAkMag.transform.position, Quaternion.identity);
        }
        private IEnumerator DisablePlayer()
        {
            yield return new WaitForSeconds(3.5f);
            this.gameObject.SetActive(false);
        }
        public void EnablePlayer()
        {
            this.gameObject.SetActive(true);
            m_MyGun.gameObject.SetActive(true);
        }
        public void Aim()
        {
            if (m_Aim == false)
            {
                m_Aim = true;
                m_OtherPlayerAnimator.SetTrigger("StartAim");
                m_OtherPlayerAnimator.SetBool("Aim", m_Aim);
            }
        }
        public void StopAim()
        {
            if (m_Aim)
            {
                m_Aim = false;
                m_OtherPlayerAnimator.SetBool("Aim", m_Aim);
            }
        }
        public void Run()
        {
            m_OtherPlayerAnimator.SetBool("Run", true);
        }
        public void StopRun()
        {
            m_OtherPlayerAnimator.SetBool("Run", false);
        }
        public void Shoot()
        {
            m_OtherPlayerAnimator.SetTrigger("Shoot");
            AudioSource.PlayClipAtPoint(m_ShootAudio, this.gameObject.transform.position, 0.3f);
        }
        public void Reload()
        {
            m_OtherPlayerAnimator.SetTrigger("Reload");
            m_AudioSource.clip = m_ReloadAudio;
            m_AudioSource.Play();
        }
        public void Movement(float X, float Z)
        {
            if ((X == 0.707107f || X == -0.707107f) && (Z == 0.707107f || Z == -0.707107f))
            {
                if (X < 0)
                    X = -1;
                else
                    X = 1;
                if (Z < 0)
                    Z = -1;
                else
                    Z = 1;
                /*
                m_CurrentAnimationBlendVector = Vector2.SmoothDamp(m_CurrentAnimationBlendVector, new Vector2(X, Z), ref m_AnimationVelocity, m_AnimationSmoothTime);
                m_OtherPlayerAnimator.SetFloat("MoveX", m_CurrentAnimationBlendVector.x);
                m_OtherPlayerAnimator.SetFloat("MoveZ", m_CurrentAnimationBlendVector.y);
                */
            }//Para poner en 1 los valores que envia un usuario de movil
            else if ((X != 1f || X != -1f) && (Z != 1f || Z != -1f) && (Z != 0 && X != 0))
            {
                if (X < -0.5f)
                    X = -1;
                else if (X > 0.5f)
                    X = 1;
                if (Z < -0.5f)
                    Z = -1;
                else if (Z > 0.5f)
                    Z = 1;
            }
            if (X != m_CurrentAnimationBlendVector.x || Z != m_CurrentAnimationBlendVector.y)
            {
                m_CurrentAnimationBlendVector = Vector2.SmoothDamp(m_CurrentAnimationBlendVector, new Vector2(X, Z), ref m_AnimationVelocity, m_AnimationSmoothTime);
                m_OtherPlayerAnimator.SetFloat("MoveX", m_CurrentAnimationBlendVector.x);
                m_OtherPlayerAnimator.SetFloat("MoveZ", m_CurrentAnimationBlendVector.y);
            }
        }
    }
}
