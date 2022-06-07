using Project.Managers;
using Project.Networking;
using Project.PlayerM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Project.AI
{
    public class ZombieManager : MonoBehaviour
    {
        [SerializeField]
        private NetworkIdentity m_NetworkIdentity;

        public enum TState
        { 
            FIND_PLAYER = 0,
            GOING,
            ATTACK,
            DIE,
        }

        [Header("GlobalZombieParameter")]
        public TState m_State;
        public Animator m_ZombieAnimator;
        public float m_AttackCooldown;
        private float m_CurrentAttackCooldown;
        List<Transform> m_Players;
        List<string> m_PlayerId;
        int m_CurrentPlayerTracked;
        public float m_TrackPlayerCooldown = 0.7f;
        private float m_CurrentTrackPlayerCooldown;
        private PlayerID m_PlayerID;
        public AudioSource m_AudioSource;
        public List<AudioClip> m_Sounds;
        public float m_ZombieSoundCooldown = 2f;
        private float m_CurrentZombieSoundCooldown;

        [Header("IntelligentZombie")]
        private NavMeshAgent m_navMesh;
        Transform m_PlayerTracked;
        private Player m_Player;
        private Zombie m_ZombieRot;
        private bool m_SendInfo=true;

        [Header("DumbZombie")]
        Vector3 m_OldPosition;
        public float m_PositionSpeedCooldown=0.2f;
        private float m_CurrentPositionSpeedCooldown;
        public float m_GrabPlayersCooldown = 3.0f;
        private float m_CurrentGrabPlayersCooldown;
        public float m_NoMovingPositionCooldown=1.0f;
        private float m_CurrentNoMovingPositionCooldown;
        bool m_AlreadyDead;

        private void Awake()
        {
            m_AlreadyDead = false;
        }
        private void Start()
        {
            m_OldPosition = transform.position;
            m_navMesh = GetComponent<NavMeshAgent>();
            m_NetworkIdentity = GetComponent<NetworkIdentity>();
            m_Player = new Player();
            m_Player.position = new Position();
            m_Player.position.x = 0;
            m_Player.position.y = 0;
            m_Player.position.z = 0;
            m_ZombieRot = new Zombie();
            m_ZombieRot.zombieRotation = 0;
            m_PlayerID = new PlayerID();
            m_PlayerID.id = "";
        }
        private void Update()
        {
            if (GameController.GetGameController().GetHost())
            {          
                switch (m_State)
                {
                    case TState.FIND_PLAYER:
                        UpdateFindPlayer();
                        break;
                    case TState.GOING:
                        UpdateGoing();
                        break;
                    case TState.ATTACK:
                        UpdateAttack();
                        break;
                    case TState.DIE:
                        break;
                }           
            }
            else            
                UpdateDumbZombie();

            UpdateZombieSounds();
        }
        private void FixedUpdate()
        {
            if (GameController.GetGameController().GetHost())
            {
                if (m_SendInfo)
                    SendPosition();
            }
        }
        private void UpdateFindPlayer()
        {
            m_Players=GameController.GetGameController().GetPlayers();
            m_PlayerId = GameController.GetGameController().GetPlayerIDS();

            FindClosestPlayer();
            if(m_Players.Count>0)
             ChangeState(TState.GOING);
        }
        private void UpdateGoing()
        {
            if ((m_PlayerTracked.transform.position - transform.position).magnitude < 1.0f)
            {
                ChangeState(TState.ATTACK);
            }
            m_CurrentGrabPlayersCooldown -= Time.deltaTime;
            if (m_CurrentGrabPlayersCooldown <= 0)
            {
                m_CurrentGrabPlayersCooldown = m_GrabPlayersCooldown;
                m_Players = GameController.GetGameController().GetPlayers();
                m_PlayerId = GameController.GetGameController().GetPlayerIDS();
            }
            m_CurrentTrackPlayerCooldown -= Time.deltaTime;
            if (m_CurrentTrackPlayerCooldown <= 0)
            {
                FindClosestPlayer();
                m_navMesh.destination = m_PlayerTracked.position;
            }
        }
        private void UpdateAttack()
        {
            m_CurrentAttackCooldown -= Time.deltaTime;
            if ((m_PlayerTracked.transform.position - transform.position).magnitude > 1.0f)
            {
                ChangeState(TState.FIND_PLAYER);
            }
            if (m_CurrentAttackCooldown <= 0 && m_AlreadyDead==false)
            {
                m_PlayerID.id = m_PlayerId[m_CurrentPlayerTracked];
                m_Players[m_CurrentPlayerTracked].GetComponent<Gun>().DamageReceived();
                m_NetworkIdentity.GetSocket().Emit("playerHit", new JSONObject(JsonUtility.ToJson(m_PlayerID)));
                m_CurrentAttackCooldown = m_AttackCooldown;            
                m_ZombieAnimator.SetTrigger("Attack");   
            }
        }
        public void SetZombieRotation(float value)
        {
            transform.localEulerAngles = new Vector3(0, value, 0);
        }
        public void SendPosition()
        {
            m_Player.position.x = Mathf.Round(transform.position.x * 1000.0f) / 1000.0f;
            m_Player.position.y = Mathf.Round(transform.position.y * 1000.0f) / 1000.0f;
            m_Player.position.z = Mathf.Round(transform.position.z * 1000.0f) / 1000.0f;
            m_Player.id = m_NetworkIdentity.GetID();
            m_NetworkIdentity.GetSocket().Emit("updateZombiePosition", new JSONObject(JsonUtility.ToJson(m_Player)));
            m_ZombieRot.zombieRotation = transform.localEulerAngles.y;
            m_ZombieRot.id = m_NetworkIdentity.GetID();
            m_NetworkIdentity.GetSocket().Emit("updateZombieRotation", new JSONObject(JsonUtility.ToJson(m_ZombieRot)));
        }
        private void UpdateZombieSounds()
        {
            m_CurrentZombieSoundCooldown -= Time.deltaTime;
            if (m_CurrentZombieSoundCooldown <= 0)
            {
                m_CurrentZombieSoundCooldown = m_ZombieSoundCooldown;
                int l_Random = Random.Range(0, 2);
                m_AudioSource.clip = m_Sounds[l_Random];
                m_AudioSource.Play();
            }
        }
        private void UpdateDumbZombie()
        {
            if (m_SendInfo) 
            {
                if (m_PlayerTracked != null)
                {
                    m_navMesh.destination = m_PlayerTracked.position;
                }
                m_CurrentPositionSpeedCooldown -= Time.deltaTime;
                m_CurrentGrabPlayersCooldown -= Time.deltaTime;
                m_CurrentTrackPlayerCooldown -= Time.deltaTime;
                m_CurrentAttackCooldown -= Time.deltaTime;

                if (m_CurrentPositionSpeedCooldown <= 0)
                {
                    m_OldPosition = transform.position;
                    m_CurrentPositionSpeedCooldown = m_PositionSpeedCooldown;
                }   
                
                if ((transform.position - m_OldPosition).magnitude > 0.4f)
                {
                    m_ZombieAnimator.SetBool("Walk", true);
                    m_CurrentNoMovingPositionCooldown = 0;
                }
                else
                {
                    m_CurrentNoMovingPositionCooldown += Time.deltaTime;
                    if (m_CurrentNoMovingPositionCooldown >= m_NoMovingPositionCooldown)
                    {
                        m_ZombieAnimator.SetBool("Walk", false);
                    }
                }

                if (m_CurrentGrabPlayersCooldown<=0)
                {
                    m_CurrentGrabPlayersCooldown = m_GrabPlayersCooldown;
                    m_Players = GameController.GetGameController().GetPlayers();
                    m_PlayerId = GameController.GetGameController().GetPlayerIDS();
                }

                if (m_CurrentTrackPlayerCooldown <= 0)
                {
                    m_CurrentTrackPlayerCooldown = m_TrackPlayerCooldown;
                    FindClosestPlayer();
                }
                if (m_PlayerTracked != null)
                {
                    if ((m_PlayerTracked.position - transform.position).magnitude < 1.0)
                    {
                        if (m_CurrentAttackCooldown <= 0)
                        {
                            m_CurrentAttackCooldown = m_AttackCooldown;
                            if (m_AlreadyDead == false)
                                m_ZombieAnimator.SetTrigger("Attack");
                            m_Players[m_CurrentPlayerTracked].GetComponent<Gun>().DamageReceived();
                        }
                    }
                }
            }
        }
        private void ChangeState(TState NewState)
        {
            //Exit Logic
            switch (m_State) 
            {
                case TState.FIND_PLAYER:
                    break;
                case TState.GOING:
                    m_ZombieAnimator.SetBool("Walk", false);
                    break;
                case TState.ATTACK:
                    m_navMesh.isStopped = false;
                    break;
                case TState.DIE:
                    break;
            }
            //Enter Logic
            switch (NewState)
            {
                case TState.FIND_PLAYER:
                    break;
                case TState.GOING:
                    m_ZombieAnimator.SetBool("Walk", true);
                    m_navMesh.destination = m_PlayerTracked.position;
                    break;
                case TState.ATTACK:
                    m_navMesh.isStopped = true;
                    break;
                case TState.DIE:
                    m_SendInfo = false;
                    m_navMesh.isStopped = true;
                    if(m_AlreadyDead == false)
                    {
                        m_AlreadyDead = true;
                        m_ZombieAnimator.ResetTrigger("Attack");
                        m_ZombieAnimator.SetTrigger("Die");
                        foreach (Collider _Collider in GetComponentsInChildren<Collider>())
                        {
                            _Collider.enabled = false;
                        }
                    }
                    m_PlayerTracked = null;
                    break;
            }
            m_State = NewState;
        }
        public void FindClosestPlayer()
        {
            if(m_Players.Count>0)
            {
                float l_Min = 10000;
                for (int i = 0; i < m_Players.Count; i++)
                {
                    float l_Distance = (m_Players[i].position - transform.position).magnitude;
                    if (l_Distance < l_Min)
                    {
                        l_Min = l_Distance;
                        m_PlayerTracked = m_Players[i];
                        m_CurrentPlayerTracked = i;
                    }
                }
            }
            else
            {
                if(GameController.GetGameController().GetHost())
                {
                    m_SendInfo = false;
                    //m_navMesh.isStopped = true;
                    ChangeState(TState.FIND_PLAYER);
                }           
                m_PlayerTracked = null;
            }
        }
        public void DisableAfterDie()
        {
            gameObject.SetActive(false);
        }
        public void SetDieState()
        {
            if (m_SendInfo)
            {
                ChangeState(TState.DIE);
                m_SendInfo = false;
            }
        }
        public void Die()
        {
            if (m_SendInfo)
            {
                if(m_navMesh.destination!=null)
                    m_navMesh.isStopped = true;
                m_SendInfo = false;
                m_ZombieAnimator.ResetTrigger("Attack");
                m_ZombieAnimator.SetTrigger("Die");
                foreach(Collider _Collider in GetComponentsInChildren<Collider>())
                {
                    _Collider.enabled = false;
                }
            }
        }
        public void Respawn()
        {
            foreach (Collider _Collider in GetComponentsInChildren<Collider>())
            {
                _Collider.enabled = true;
            }
            transform.position=GameController.GetGameController().GetSpawn().position;
            m_AlreadyDead = false;
            m_SendInfo = true;
            ChangeState(TState.FIND_PLAYER);
        }
        public void OnePlayerDied()
        {
            Debug.Log("Cambiar objetivo");
            m_Players = GameController.GetGameController().GetPlayers();
            m_PlayerId = GameController.GetGameController().GetPlayerIDS();

            FindClosestPlayer();
        }
    }
}

