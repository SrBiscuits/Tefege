using Project.Networking;
using Project.PlayerM;
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
        public Animator m_Animator;

        [Header("Gun Attributes")]
        public int m_AmmoReserved = 90; // MUNICIÓN DE RESERVA
        public int m_CurrentAmmo; // MUNICIÓN DEL CARGADOR
        public float m_NormalDamage = 33.0f;
        public float m_HeadDamage = 51.0f;
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
                m_PlayerCanvas = Instantiate(m_PlayerCanvas, transform);
                m_PlayerInput.Player.Fire.started += ShootTrue;
                m_PlayerInput.Player.Aim.started += AimTrue;
                m_PlayerInput.Player.Fire.canceled += ShootFalse;
                m_PlayerInput.Player.Aim.canceled += AimFalse;
                m_PlayerInput.Player.Reload.performed += Reload;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (m_NetworkIdentity.IsControlling())
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
            m_AmmoReserved = 120;
            m_BulletCounter = 0;
            m_PlayerCanvas.SetCurrentAmmo(m_CurrentAmmo);
            m_PlayerCanvas.SetReservedAmmo(m_AmmoReserved);
        }
        public void Reload(InputAction.CallbackContext Context)
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
                m_PlayerCanvas.SetCurrentAmmo(m_CurrentAmmo);
                m_PlayerCanvas.SetReservedAmmo(m_AmmoReserved);
                m_Animator.SetTrigger("Reload");
                m_Reloading = true;
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
                m_PlayerCanvas.SetCurrentAmmo(m_CurrentAmmo);
                m_BulletData.activator = NetworkClient.ClientID;
                m_BulletData.position.x = Mathf.Round(m_ShootTransform.transform.position.x * 1000.0f) / 1000.0f;
                m_BulletData.position.y = Mathf.Round(m_ShootTransform.transform.position.y * 1000.0f) / 1000.0f;
                m_BulletData.position.z = Mathf.Round(m_ShootTransform.transform.position.z * 1000.0f) / 1000.0f;
                m_BulletData.direction.x = Mathf.Round(m_Player.GetBulletRot().x * 1000.0f) / 1000.0f;
                m_BulletData.direction.y = Mathf.Round(m_Player.GetBulletRot().y * 1000.0f) / 1000.0f;
                m_BulletData.direction.z = Mathf.Round(m_Player.GetBulletRot().z * 1000.0f) / 1000.0f;
              
                m_NetworkIdentity.GetSocket().Emit("fireBullet", new JSONObject(JsonUtility.ToJson(m_BulletData)));

                if (m_AimisBeingPerformed)
                    m_Animator.SetTrigger("ShootAim");
                else
                    m_Animator.SetTrigger("Shoot");

                /*
                GameObject l_Bullet = CPoolElements.SharedInstance.GetDecal();
                if (l_Bullet != null)
                {
                    //l_Bullet.gameObject.GetComponent<Bullet>().Damage(33);
                    l_Bullet.transform.position = m_ShootTransform.transform.position;
                    l_Bullet.transform.rotation = m_ShootTransform.transform.rotation;             
                    l_Bullet.SetActive(true);
                }
               
                Debug.Log("a2");
                */
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
            m_AimisBeingPerformed = true;
        }
        private void AimFalse(InputAction.CallbackContext Context)
        {
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
    }

}

