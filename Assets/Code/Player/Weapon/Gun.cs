using Project.Managers;
using Project.Networking;
using Project.PlayerM;
using Project.Sound;
using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.PlayerM
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class Gun : MonoBehaviour
    {
        private BulletData m_BulletData;
        private NetworkIdentity m_NetworkIdentity;
        public GameObject m_ShootTransform;
        PlayerKeyCode m_PlayerInput;
        public PlayerManager m_Player;
        public PlayerCanvas m_PlayerCanvas;
        public MobilePlayerCanvas m_MobilePlayerCanvas;
        public Animator m_Animator;
        public AudioSource m_AudioSource;
        public AudioClip m_ReloadAudio;
        public AudioClip m_ShootAudio;

        [Header("Mobile Stuff")]
        bool m_IsMovile;

        [Header("Gun Attributes")]
        public int m_AmmoReserved = 90; // MUNICIÓN DE RESERVA
        public int m_CurrentAmmo; // MUNICIÓN DEL CARGADOR
        public float m_ShootTime = 0.16f;
        private float m_CurrentShootedTime;
        private bool m_Reloading;
        private int m_BulletCounter;
        bool m_ShootisBeingPerformed;
        bool m_AimisBeingPerformed;

        private void Awake()
        {
            m_PlayerInput = new PlayerKeyCode();
        }
        private void Start()
        {
            m_NetworkIdentity = GetComponent<NetworkIdentity>();
            m_CurrentShootedTime = m_ShootTime;
            m_CurrentAmmo = 30;

            NetworkClient.m_OnAmmoPick += MaxAmmo;

            m_BulletData = new BulletData();
            m_BulletData.position = new Position();
            m_BulletData.direction = new Position();
            if (m_NetworkIdentity.IsControlling())
            {
                if (!GameController.GetGameController().IsPlatformPc())
                    m_IsMovile = true;
                if (m_IsMovile)
                {
                    m_MobilePlayerCanvas = Instantiate(m_MobilePlayerCanvas, transform);
                    m_Player.SetMobilePlayerCanvas(m_MobilePlayerCanvas);
                }                
                else
                {
                    m_PlayerCanvas = Instantiate(m_PlayerCanvas, transform);
                    m_PlayerInput.Player.Fire.started += ShootTrue;
                    m_PlayerInput.Player.Aim.started += AimTrue;
                    m_PlayerInput.Player.Fire.canceled += ShootFalse;
                    m_PlayerInput.Player.Aim.canceled += AimFalse;
                    m_PlayerInput.Player.Reload.performed += Reload;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (m_NetworkIdentity.IsControlling() && m_Player.GetAlive())
            {
                if (m_AimisBeingPerformed)
                    Aim();
                else
                    m_Animator.SetBool("Aiming", false);
                if (m_ShootisBeingPerformed)
                    Shoot();

                m_CurrentShootedTime -= Time.deltaTime;

            }
        }
        public void FinishReload()
        {
            m_Reloading = false;
        }
        public void AddAmmo()
        {
            if (m_AmmoReserved == 0)
            {
                m_BulletCounter = (30 - m_CurrentAmmo);
            }
            m_AmmoReserved += 30;
        }
        private void MaxAmmo(SocketIOEvent e)
        {
            m_CurrentAmmo = 30;
            m_AmmoReserved = 240;
            m_BulletCounter = 0;
            if(m_IsMovile)
            {
                m_MobilePlayerCanvas.SetCurrentAmmo(m_CurrentAmmo);
                m_MobilePlayerCanvas.SetReservedAmmo(m_AmmoReserved);
            }
            else
            {
                m_PlayerCanvas.SetCurrentAmmo(m_CurrentAmmo);
                m_PlayerCanvas.SetReservedAmmo(m_AmmoReserved);
            }
            PlayerSounds l_Sound = GetComponent<PlayerSounds>();
            l_Sound.MaxAmmo();
        }
        public void Reload(InputAction.CallbackContext Context)
        {
            if (!m_Player.GetRun())
            {
                if (m_AmmoReserved != 0 && m_CurrentAmmo != 30 && !m_Reloading)
                {
                    if (m_AmmoReserved < 30)
                    {
                        if (m_BulletCounter > m_AmmoReserved)
                        {
                            m_BulletCounter = m_AmmoReserved;
                            m_AmmoReserved -= m_BulletCounter;
                            m_CurrentAmmo += m_BulletCounter;
                            m_BulletCounter = 0;
                        }
                        else
                        {
                            m_AmmoReserved -= m_BulletCounter;
                            m_CurrentAmmo += m_BulletCounter;
                            m_BulletCounter = 0;
                        }
                    }
                    else
                    {
                        m_AmmoReserved -= m_BulletCounter;
                        m_CurrentAmmo += m_BulletCounter;
                        m_BulletCounter = 0;
                    }
                    if (m_IsMovile)
                    {
                        m_MobilePlayerCanvas.SetCurrentAmmo(m_CurrentAmmo);
                        m_MobilePlayerCanvas.SetReservedAmmo(m_AmmoReserved);
                    }
                    else
                    {
                        m_PlayerCanvas.SetCurrentAmmo(m_CurrentAmmo);
                        m_PlayerCanvas.SetReservedAmmo(m_AmmoReserved);
                    }
                    m_Animator.SetTrigger("Reload");
                    m_Reloading = true;
                    m_Player.SendAnimationNumber(4f);
                    m_AudioSource.clip = m_ReloadAudio;
                    m_AudioSource.Play();
                }
            }
        }
        public void Aim()
        {
            m_Animator.SetBool("Aiming", true);
        }
        public void Shoot()
        {
            if (m_CurrentShootedTime < 0 && m_CurrentAmmo > 0 && !m_Reloading && !m_Player.GetRun())
            {
                m_CurrentAmmo--;
                m_BulletCounter++;
                m_CurrentShootedTime = m_ShootTime;
                if(m_IsMovile)
                    m_MobilePlayerCanvas.SetCurrentAmmo(m_CurrentAmmo);
                else
                    m_PlayerCanvas.SetCurrentAmmo(m_CurrentAmmo);
                m_BulletData.position.x = Mathf.Round(m_ShootTransform.transform.position.x * 1000.0f) / 1000.0f;
                m_BulletData.position.y = Mathf.Round(m_ShootTransform.transform.position.y * 1000.0f) / 1000.0f;
                m_BulletData.position.z = Mathf.Round(m_ShootTransform.transform.position.z * 1000.0f) / 1000.0f;
                m_BulletData.direction.x = Mathf.Round(m_Player.GetBulletRot().x * 1000.0f) / 1000.0f;
                m_BulletData.direction.y = (Mathf.Round(m_Player.GetBulletRot().y * 1000.0f) / 1000.0f);
                m_BulletData.direction.z = Mathf.Round(m_Player.GetBulletRot().z * 1000.0f) / 1000.0f;
              
                m_NetworkIdentity.GetSocket().Emit("fireBullet", new JSONObject(JsonUtility.ToJson(m_BulletData)));

                if (m_AimisBeingPerformed)
                    m_Animator.SetTrigger("ShootAim");
                else
                    m_Animator.SetTrigger("Shoot");

                AudioSource.PlayClipAtPoint(m_ShootAudio, this.gameObject.transform.position,0.3f);

                m_Player.SendAnimationNumber(3f);
            }
        }
        private void ShootTrue(InputAction.CallbackContext Context)
        {
            m_ShootisBeingPerformed = true;
        }
        private void ShootFalse(InputAction.CallbackContext Context)
        {
            m_ShootisBeingPerformed = false;
        }
        private void AimTrue(InputAction.CallbackContext Context)
        {
            if (m_AimisBeingPerformed==false)
                m_Player.SendAnimationNumber(1f);
            m_AimisBeingPerformed = true;
        }
        private void AimFalse(InputAction.CallbackContext Context)
        {
            if (m_AimisBeingPerformed)
                m_Player.SendAnimationNumber(5f);
            m_AimisBeingPerformed = false;  
        }
        private void OnEnable()
        {
            m_PlayerInput.Enable();
        }
        private void OnDisable()
        {
            m_PlayerInput.Disable();
        }
        public bool GetAim()
        {
            return m_AimisBeingPerformed;
        }
        public void NextRound()
        {
            int l_Round = GameController.GetGameController().GetRound();
            if (l_Round < 6)
            {
                if (m_IsMovile)
                {
                    if (l_Round == 2)
                        m_MobilePlayerCanvas.SetRound("II");
                    if (l_Round == 3)
                        m_MobilePlayerCanvas.SetRound("III");
                    if (l_Round == 4)
                        m_MobilePlayerCanvas.SetRound("IV");
                    if (l_Round == 5)
                        m_MobilePlayerCanvas.SetRound("V");
                }
                else
                {
                    if (l_Round == 2)
                        m_PlayerCanvas.SetRound("II");
                    if (l_Round == 3)
                        m_PlayerCanvas.SetRound("III");
                    if (l_Round == 4)
                        m_PlayerCanvas.SetRound("IV");
                    if (l_Round == 5)
                        m_PlayerCanvas.SetRound("V");
                }
            }
            else
            {
                if(m_IsMovile)
                    m_MobilePlayerCanvas.SetRound(l_Round.ToString());
                else
                    m_PlayerCanvas.SetRound(l_Round.ToString());
            }
        }
        public void DamageReceived()
        {
            if(m_NetworkIdentity.IsControlling())
            {
                if (m_IsMovile)
                    m_MobilePlayerCanvas.DamageReceived();
                else
                    m_PlayerCanvas.DamageReceived();
            }
        }
        public void Back2Live()
        {
            if (m_IsMovile)
                m_MobilePlayerCanvas.FullHealth();
            else
                m_PlayerCanvas.FullHealth();
        }
        public void AimButton()
        {
            if(m_AimisBeingPerformed==false)
                m_Player.SendAnimationNumber(1f);
            else
                m_Player.SendAnimationNumber(5f);
            m_AimisBeingPerformed = !m_AimisBeingPerformed;
        }
        public void ShootButton()
        {
            m_ShootisBeingPerformed = true;
        }
        public void EndShootButton()
        {
            m_ShootisBeingPerformed = false;
        }
        public void MobileReload()
        {
            if (!m_Player.GetRun())
            {
                if (m_AmmoReserved != 0 && m_CurrentAmmo != 30 && !m_Reloading)
                {
                    if (m_AmmoReserved < 30)
                    {
                        if (m_BulletCounter > m_AmmoReserved)
                        {
                            m_BulletCounter = m_AmmoReserved;
                            m_AmmoReserved -= m_BulletCounter;
                            m_CurrentAmmo += m_BulletCounter;
                            m_BulletCounter = 0;
                        }
                        else
                        {
                            m_AmmoReserved -= m_BulletCounter;
                            m_CurrentAmmo += m_BulletCounter;
                            m_BulletCounter = 0;
                        }
                    }
                    else
                    {
                        m_AmmoReserved -= m_BulletCounter;
                        m_CurrentAmmo += m_BulletCounter;
                        m_BulletCounter = 0;
                    }
                    if (m_IsMovile)
                    {
                        m_MobilePlayerCanvas.SetCurrentAmmo(m_CurrentAmmo);
                        m_MobilePlayerCanvas.SetReservedAmmo(m_AmmoReserved);
                    }
                    else
                    {
                        m_PlayerCanvas.SetCurrentAmmo(m_CurrentAmmo);
                        m_PlayerCanvas.SetReservedAmmo(m_AmmoReserved);
                    }
                    m_Animator.SetTrigger("Reload");
                    m_Reloading = true;
                    m_Player.SendAnimationNumber(4f);
                    m_AudioSource.clip = m_ReloadAudio;
                    m_AudioSource.Play();
                }
            }
        }
    }
}

