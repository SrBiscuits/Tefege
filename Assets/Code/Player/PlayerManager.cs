using Project.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.PlayerM
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Axis")]
        float m_Yaw;
        float m_Pitch;
        public Transform m_PitchController;
        public float m_MinPitch = -35.0f;
        public float m_MaxPitch = 105.0f;
        public float m_YawRotationalSpeed = 0.1f;
        public float m_PitchRotationalSpeed = 0.1f;
        public bool m_InvertHorizontalAxis;
        public bool m_InvertVerticalAxis;
        public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;
        public KeyCode m_DebugLockKeyCode = KeyCode.O;
        private bool m_AngleLocked = false;
        private bool m_AimLocked = true;

        [Header("Ground/Gravity")]
        public float m_VerticalSpeed;
        bool m_Grounded;
        float m_TimeSinceLastGround;

        [Header("Values")]
        [SerializeField]
        public float m_Speed;
        public float m_RunSpeedAdd;
        bool m_RunisBeingPerformed;
        bool m_RunStarted;

        [Header("Class References")]
        [SerializeField]
        private NetworkIdentity m_NetworkIdentity;
        PlayerKeyCode m_PlayerInput;
        public CharacterController m_CharacterController;
        public Animator m_Animator;
        Gun m_Gun;

        [Header("Multiplayer")]
        private float m_LastRotation;
        private float m_CurrentSpawnerZombieCooldown=0.5f;
        public List<Transform> m_PosibleZombieSpawn;

        private void Awake()
        {
            m_PlayerInput = new PlayerKeyCode();
            m_Gun = GetComponent<Gun>();
        }
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.lockState = CursorLockMode.Confined;
            m_Yaw = transform.rotation.eulerAngles.y;
            m_Pitch = m_PitchController.localRotation.eulerAngles.x;
            Cursor.lockState = CursorLockMode.Locked;

            m_VerticalSpeed = 0.0f;

            if (m_NetworkIdentity.IsControlling())
            {
                m_PlayerInput.Player.Run.started += RunTrue;
                m_PlayerInput.Player.Run.canceled += RunFalse;
            }
        }
        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(m_DebugLockAngleKeyCode))
                m_AngleLocked = !m_AngleLocked;
            if (Input.GetKeyDown(m_DebugLockKeyCode))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                    Cursor.lockState = CursorLockMode.None;
                else
                    Cursor.lockState = CursorLockMode.Locked;
                m_AimLocked = Cursor.lockState == CursorLockMode.Locked;
            }
#endif
            if (m_NetworkIdentity.IsControlling())
            {
                CheckMovement();
                CheckAxisMovement();
                UpdateZombieSpawner();
            }

            UpdateGravity();
           
            m_CurrentSpawnerZombieCooldown -= Time.deltaTime;
        }
        private void FixedUpdate()
        {
            m_Grounded = false;
        }
        public float GetLastRotation()
        {
            return m_LastRotation;
        }
        public Vector3 GetBulletRot()
        {
            return new Vector3(GetLastRotation(), transform.localEulerAngles.y, 0);
        }
        public void SetRotation(float GunValue,float PlayerValue)
        {
            m_PitchController.rotation = Quaternion.Euler(GunValue, PlayerValue, 0);
        }
        private void CheckMovement()
        {
            float l_X = m_PlayerInput.Player.Movement.ReadValue<Vector2>().x;
            float l_Z = m_PlayerInput.Player.Movement.ReadValue<Vector2>().y;

            if (l_X != 0 || l_Z != 0)          
                m_Animator.SetBool("Moving", true);           
            else
                m_Animator.SetBool("Moving", false);

            if (m_CharacterController != null)
            {
                if (m_RunisBeingPerformed && m_RunStarted==false)
                {
                    m_Speed += m_RunSpeedAdd;
                    m_RunStarted = true;
                }
                Vector3 l_Move = transform.right * l_X + transform.forward * l_Z;
                m_CharacterController.Move(l_Move * m_Speed * Time.deltaTime);
            }         
        }
        private void CheckAxisMovement()
        {
            float l_MouseAxisX = m_PlayerInput.Player.Camera.ReadValue<Vector2>().x;
            float l_MouseAxisY = m_PlayerInput.Player.Camera.ReadValue<Vector2>().y;

            if (m_InvertHorizontalAxis) l_MouseAxisX = -l_MouseAxisX;
            if (m_InvertVerticalAxis) l_MouseAxisY = -l_MouseAxisY;

            if (!m_AngleLocked)
            {
                m_Yaw = m_Yaw + l_MouseAxisX * m_YawRotationalSpeed;
                m_Pitch = m_Pitch + l_MouseAxisY * m_PitchRotationalSpeed;
                m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
            }
            transform.rotation = Quaternion.Euler(0.0f, m_Yaw, 0.0f);
            m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0.0f, 0.0f);

            m_LastRotation = m_Pitch;
        }
        private void UpdateGravity()
        {
            m_VerticalSpeed = m_VerticalSpeed + (Physics.gravity.y * 2) * Time.deltaTime;

            Vector3 l_Move = new Vector3(0.0f, 1*m_VerticalSpeed, 0.0f);
            if (m_CharacterController != null)
            {
                m_CharacterController.Move(l_Move * Time.deltaTime);
            }

            CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Move);
            m_TimeSinceLastGround -= Time.deltaTime;
            if ((l_CollisionFlags & CollisionFlags.Below) != 0 && m_VerticalSpeed <= 0.0f)
            {
                m_VerticalSpeed = 0.0f;
                m_Grounded = true;
                m_TimeSinceLastGround = 0.2f;
            }
            else if (m_TimeSinceLastGround < 0)
                m_Grounded = false;
        }
        private void UpdateZombieSpawner()
        {         
            if (m_CurrentSpawnerZombieCooldown <= 0)
            {
                m_PosibleZombieSpawn = GameController.GetGameController().GetAllZombieSpawner();
                float l_MinDistance=50.0f;
                int l_MinIndex=0;
                for(int i = 0; i < m_PosibleZombieSpawn.Count; ++i)
                {
                    if ((m_PosibleZombieSpawn[i].position - transform.position).magnitude < l_MinDistance)
                    {                
                        l_MinDistance = (m_PosibleZombieSpawn[i].position - transform.position).magnitude;
                        l_MinIndex = i;
                    }
                }
                GameController.GetGameController().SetCloseSpawner(m_PosibleZombieSpawn[l_MinIndex]);
                m_CurrentSpawnerZombieCooldown = Random.Range(4, 10);
            }            
        }
        private void OnEnable()
        {
            m_PlayerInput.Enable();
        }
        private void OnDisable()
        {
            m_PlayerInput.Disable();
        }
        private void RunTrue(InputAction.CallbackContext Context)
        {
            if(!m_Gun.GetAim())
            {
                m_RunisBeingPerformed = true;
                m_Animator.SetBool("Runing", true);
            }
        }
        private void RunFalse(InputAction.CallbackContext Context)
        {
            if (!m_Gun.GetAim())            
                m_Speed -= m_RunSpeedAdd;

            m_RunisBeingPerformed = false;
            m_Animator.SetBool("Runing", false);
            m_RunStarted = false;
        }
        public bool GetRun()
        {
            return m_RunisBeingPerformed;
        }
    }
}

