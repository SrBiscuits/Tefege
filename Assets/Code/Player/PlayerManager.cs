using Project.Managers;
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
        float m_StartYaw;
        float m_StartPitch;
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
        bool m_MovingAxis;

        [Header("Ground/Gravity")]
        public float m_VerticalSpeed;

        [Header("Values")]
        [SerializeField]
        public float m_Speed;
        float m_OriginalSpeed;
        public float m_RunSpeedAdd;
        bool m_RunisBeingPerformed;
        bool m_RunStarted;
        bool m_Alive = true;
        bool m_Revive = false;

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
        public GameObject m_DeadPlayerCamera;
        public GameObject m_AkMag;
        public GameObject m_MyAkMag;
        MobilePlayerCanvas m_MobilePlayerCanvas;
        float m_X;
        float m_Z;


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
            m_StartYaw = m_Yaw;
            m_Pitch = m_StartPitch;
            Cursor.lockState = CursorLockMode.Locked;

            m_VerticalSpeed = 0.0f;
            m_OriginalSpeed = m_Speed;
            if (m_NetworkIdentity.IsControlling() && m_Alive)
            {
                m_PlayerInput.Player.Run.started += RunTrue;
                m_PlayerInput.Player.Run.canceled += RunFalse;
            }

            if (!GameController.GetGameController().IsPlatformPc())
            {
                m_YawRotationalSpeed *= 5;
                m_PitchRotationalSpeed *= 5;
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
            if (m_NetworkIdentity.IsControlling() && m_Alive)
            {
                CheckMovement();
                CheckAxisMovement();
                UpdateZombieSpawner();
                UpdateGravity();
            }         
            m_CurrentSpawnerZombieCooldown -= Time.deltaTime;
        }
        private void FixedUpdate()
        {
            if(m_NetworkIdentity.IsControlling() && m_Alive)
                SendAnimationDirection(m_X, m_Z);
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
            m_X = l_X;
            m_Z = l_Z;

            if (!GameController.GetGameController().IsPlatformPc() && l_X < 0.5f && l_Z > 0.5f)
                m_MobilePlayerCanvas.EnableRun();
            else if(!GameController.GetGameController().IsPlatformPc())
                m_MobilePlayerCanvas.DisableRun();

           

            if (l_X != 0 || l_Z != 0)          
                m_Animator.SetBool("Moving", true);           
            else
                m_Animator.SetBool("Moving", false);

            if (m_CharacterController != null)
            {
                if (m_RunisBeingPerformed && m_RunStarted==false && l_Z==1 && l_X==0)
                {
                    m_Speed += m_RunSpeedAdd;
                    m_RunStarted = true;
                    SendAnimationNumber(2f);
                    m_Animator.SetBool("Runing", true);
                }
                Vector3 l_Move = transform.right * l_X + transform.forward * l_Z;
                m_CharacterController.Move(new Vector3(l_Move.x,0f,l_Move.z) * m_Speed * Time.deltaTime);
            }     
            if(m_RunisBeingPerformed && (l_Z!=1 || l_X != 0))            
                StopRun();           
        }
        private void CheckAxisMovement()
        {
            float l_MouseAxisX = m_PlayerInput.Player.Camera.ReadValue<Vector2>().x;
            float l_MouseAxisY = m_PlayerInput.Player.Camera.ReadValue<Vector2>().y;
            if (l_MouseAxisX == 0 && l_MouseAxisY == 0)
                m_MovingAxis = false;

            if (m_InvertHorizontalAxis) l_MouseAxisX = -l_MouseAxisX;
            if (m_InvertVerticalAxis) l_MouseAxisY = -l_MouseAxisY;

            if (!m_AngleLocked || !GameController.GetGameController().IsPlatformPc())
            {
                m_Yaw = m_Yaw + l_MouseAxisX * m_YawRotationalSpeed*Time.deltaTime;
                m_Pitch = m_Pitch + l_MouseAxisY * m_PitchRotationalSpeed*Time.deltaTime;
                m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
            }
            transform.rotation = Quaternion.Euler(0.0f, m_Yaw, 0.0f);
            m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0.0f, 0.0f);

            m_LastRotation = m_Pitch;
        }
        private void UpdateGravity()
        {
            m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;

            Vector3 l_Move = new Vector3(0.0f, m_VerticalSpeed, 0.0f);
            if (m_CharacterController != null)            
                m_CharacterController.Move(l_Move * Time.deltaTime);
         

            CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Move);
            if ((l_CollisionFlags & CollisionFlags.Below) != 0 && m_VerticalSpeed <= 0.0f)          
                m_VerticalSpeed = -0.01f;
        }
        private void UpdateZombieSpawner()
        {         
            if (m_CurrentSpawnerZombieCooldown <= 0)
            {           
                if (m_PosibleZombieSpawn[0]==null)
                    m_PosibleZombieSpawn = GameController.GetGameController().GetAllZombieSpawner();

                m_PosibleZombieSpawn.Sort((a, b) => Vector3.Distance(a.position, transform.position).CompareTo(Vector3.Distance(b.position, transform.position)));
                int l_Random = Random.Range(0, 4);
                GameController.GetGameController().SetCloseSpawner(m_PosibleZombieSpawn[l_Random]);
            }            
        }
        public void DeadAnimation()
        {
            if(m_Alive)
            {
                if (m_DeadPlayerCamera == null)
                    m_DeadPlayerCamera = GameController.GetGameController().GetCamera();
                m_Animator.SetTrigger("Dead");
                m_Alive = false;
            }
        }
        public void EnableDeadPlayerCamera()
        {
            Debug.Log("Dead trigger");
            if (!m_Alive)
            {
                this.gameObject.SetActive(false);
                m_DeadPlayerCamera.SetActive(true);
            }
        }
        public void DisableDeadPlayerCamera()
        {
            m_DeadPlayerCamera.SetActive(false);
            this.gameObject.SetActive(true);
            m_Animator.SetBool("Moving", false);
            m_MaxPitch = 31.0f;
            m_Alive = true;
            m_Gun.Back2Live();
        }
        public void EnableAlive()
        {
            Debug.Log("Revive trigger");
            if (m_Revive)
            {
                m_Alive = true;
                m_Revive = false;
                m_Pitch = m_StartPitch;
                m_Yaw = m_StartYaw;
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
                m_RunisBeingPerformed = true;                       
        }
        private void RunFalse(InputAction.CallbackContext Context)
        {     
            if (m_RunisBeingPerformed)
            {
                m_Speed = m_OriginalSpeed;
                SendAnimationNumber(6f);
                m_RunisBeingPerformed = false;
                m_Animator.SetBool("Runing", false);
                m_RunStarted = false;
            }
        }
        public bool GetRun()
        {
            return m_RunisBeingPerformed;
        }
        public bool GetAlive()
        {
            return m_Alive;
        }
        public bool GetMovingAxis()
        {
            return m_MovingAxis;
        }
        public void MobileRun()
        {
            m_Speed += m_RunSpeedAdd;
            m_RunStarted = true;
            SendAnimationNumber(2f);
            m_Animator.SetBool("Runing", true);
        }
        public void StopRun()
        {
            m_Speed = m_OriginalSpeed;
            SendAnimationNumber(6f);
            m_RunisBeingPerformed = false;
            m_Animator.SetBool("Runing", false);
            m_RunStarted = false;
        }
        public void SetMobilePlayerCanvas(MobilePlayerCanvas Canvas)
        {
            m_MobilePlayerCanvas = Canvas;
        }
        public void MyAkMag()
        {
            Instantiate(m_AkMag, m_MyAkMag.transform.position, Quaternion.identity);
        }
        public void SendAnimationNumber(float Number)
        {
            m_NetworkIdentity.GetSocket().Emit("animationNumber", new JSONObject(JsonUtility.ToJson(new AnimatorNumber()
            {
                id=m_NetworkIdentity.GetID(),
                animation=Number              
            })));
        }
        public void SendAnimationDirection(float MoveX,float MoveZ)
        {
            m_NetworkIdentity.GetSocket().Emit("animationDirection", new JSONObject(JsonUtility.ToJson(new AnimatorDirection()
            {
                id=m_NetworkIdentity.GetID(),
                X=MoveX,
                Z=MoveZ
            })));
        }
    }
}

