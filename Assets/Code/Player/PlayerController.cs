using Project.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerController : MonoBehaviour
{
    float m_Yaw;
    float m_Pitch;

    [Header("Player Movement")]

    public Transform m_PitchController;
    public float m_MinPitch = -35.0f;
    public float m_MaxPitch = 105.0f;
    public float m_YawRotationalSpeed = 0.1f;
    public float m_PitchRotationalSpeed = 0.1f;
    public float m_Speed = 2.0f;
    public float m_RunSpeedMultiplier = 2f;
    public float m_VerticalSpeed = 0.0f;
    public float m_JumpSpeed = 9f;
    public float m_JumpThresholdSinceLastGround = 0.2f;
    public bool isRunning = false;
    public bool m_InvertHorizontalAxis;
    public bool m_InvertVerticalAxis;
    public bool m_OnGround = false;
    
    public CharacterController m_CharacterController;
    public PlayerVida dead;
    float m_TimeSinceLastGround = 0.0f;

   

    [Header("Input")]
    public KeyCode m_LeftKeyCode;
    public KeyCode m_RightKeyCode;
    public KeyCode m_UpKeyCode;
    public KeyCode m_DownKeyCode;
    public KeyCode m_JumpKeyCode;
    public KeyCode m_RunKeyCode = KeyCode.LeftShift;
    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;
    public KeyCode m_DebugLockKeyCode = KeyCode.O;
    private bool m_AngleLocked = false;
    private bool m_AimLocked = true;

    [Header("Gun Animator")]
    public WeaponAnimator weapAnim;

    [Header("Decal")]
    public GameObject m_HitCollisionParticlesPrefab;

    [Header("Layer")]
    public LayerMask m_ShootLayerMask;
 

    [Header("Camera")]
    public Camera m_Camera;
    [SerializeField]
    private NetworkIdentity m_NetworkIdentity;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.Confined;

        m_Yaw = transform.rotation.eulerAngles.y;
        m_Pitch = m_PitchController.localRotation.eulerAngles.x;

        m_VerticalSpeed = 0.0f;
        Cursor.lockState = CursorLockMode.Locked;
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
            MoveCharacter();
        }            
    }   

    public void MoveCharacter()
    {
        //Update yaw & pitch

        float l_MouseAxisX = Input.GetAxis("Mouse X");
        float l_MouseAxisY = Input.GetAxis("Mouse Y");

        if (m_InvertHorizontalAxis) l_MouseAxisX = -l_MouseAxisX;
        if (m_InvertVerticalAxis) l_MouseAxisY = -l_MouseAxisY;


        //x = x0 + v * dt;  ---> MRU
        if (!m_AngleLocked)
        {
            m_Yaw = m_Yaw + l_MouseAxisX * m_YawRotationalSpeed /** Time.deltaTime*/;
            m_Pitch = m_Pitch + l_MouseAxisY * m_PitchRotationalSpeed /** Time.deltaTime*/;
            m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
        }
        transform.rotation = Quaternion.Euler(0.0f, m_Yaw, 0.0f);
        m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0.0f, 0.0f); //Invert m_Pitch for vertical Rotation


        Vector3 l_Movement = Vector3.zero;
        Vector3 l_Right = transform.right;
        Vector3 l_Forward = transform.forward;

        l_Right.y = 0.0f;
        l_Right.Normalize();
        l_Forward.y = 0.0f;
        l_Forward.Normalize();

        //TECLAS DE MOVIMIENTO

        if (Input.GetKey(m_RightKeyCode) && dead.dead == false)
            l_Movement = l_Right;
        if (Input.GetKey(m_LeftKeyCode) && dead.dead == false)
            l_Movement = -l_Right;
        if (Input.GetKey(m_UpKeyCode) && dead.dead == false)
            l_Movement += l_Forward;
        if (Input.GetKey(m_DownKeyCode) && dead.dead == false)
            l_Movement += -l_Forward;

        l_Movement.Normalize();



        float l_Speed = m_Speed;

        //RUN ONLY WHEN GOING FORWARD OR SIDEWAYS

        if ((Input.GetKey(m_RunKeyCode) && !(Input.GetKey(m_DownKeyCode)) && (Input.GetKey(m_UpKeyCode))))
        {
            l_Speed *= m_RunSpeedMultiplier;
            isRunning = true;
            weapAnim.RunAnim();
        }

        else
        {
            isRunning = false;
            weapAnim.weapon.SetBool("run", false);
        }

        //JUMP

        if (Input.GetKeyDown(m_JumpKeyCode) &&/* m_OnGround*/ m_TimeSinceLastGround < m_JumpThresholdSinceLastGround)
            m_VerticalSpeed = m_JumpSpeed;

        //v = v0 + a * dt
        m_VerticalSpeed = m_VerticalSpeed + (Physics.gravity.y * 3) * Time.deltaTime;

        l_Movement = l_Movement * l_Speed * Time.deltaTime;
        l_Movement.y = m_VerticalSpeed * Time.deltaTime;

        //x = x0 + v * dt; para el plano XZ (m_Speed)
        //x = x0 + v * dt; para el plano Y (m_VerticalSpeed)

        //FLOOR AND CEILING COLLISIONS

        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Movement);

        m_TimeSinceLastGround += Time.deltaTime;

        //Comprobamos si el character colisiona solo por debajo (sin importar los lados etc) y pasamos la velocidad vertical a 0
        m_OnGround = (l_CollisionFlags & CollisionFlags.CollidedBelow) != 0;
        if (m_OnGround)
            m_TimeSinceLastGround = 0.0f;

        if (m_OnGround || ((l_CollisionFlags & CollisionFlags.CollidedAbove) != 0 && m_VerticalSpeed >= 0f))
            m_VerticalSpeed = 0.0f;

    }
}
