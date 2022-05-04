using Project.Networking;
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
            DIE
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

        [Header("IntelligentZombie")]
        private NavMeshAgent m_navMesh;
        Transform m_PlayerTracked;
        private Networking.Player m_Player;
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
            m_Player = new Networking.Player();
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
                if(m_SendInfo) 
                    SendPosition();
            }
            else            
                UpdateDumbZombie();           
        }
        private void UpdateFindPlayer()
        {
            m_Players=GameController.GetGameController().GetPlayers();
            m_PlayerId = GameController.GetGameController().GetPlayerIDS();

            FindClosestPlayer();
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
            if (m_CurrentAttackCooldown <= 0)
            {
                m_PlayerID.id = m_PlayerId[m_CurrentPlayerTracked];
                m_NetworkIdentity.GetSocket().Emit("playerHit", new JSONObject(JsonUtility.ToJson(m_PlayerID)));
                m_CurrentAttackCooldown = m_AttackCooldown;
                if (m_AlreadyDead == false)              
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
        public void UpdateDumbZombie()
        {
            if (m_SendInfo) 
            {
                m_CurrentPositionSpeedCooldown -= Time.deltaTime;
                m_CurrentGrabPlayersCooldown -= Time.deltaTime;
                m_CurrentTrackPlayerCooldown -= Time.deltaTime;

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

                if ((m_PlayerTracked.position - transform.position).magnitude < 1.0)
                {
                    if (m_CurrentAttackCooldown <= 0)
                    {
                        m_CurrentAttackCooldown = m_AttackCooldown;
                        if (m_AlreadyDead == false)                        
                            m_ZombieAnimator.SetTrigger("Attack");
                    }
                }
            }
            else
            {
                if (m_AlreadyDead == false)
                {
                    m_AlreadyDead = true;
                    m_ZombieAnimator.SetTrigger("Die");
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
                    break;
                case TState.DIE:
                    m_SendInfo = false;
                    m_navMesh.isStopped = true;
                    if(m_AlreadyDead == false)
                    {
                        m_AlreadyDead = true;
                        m_ZombieAnimator.SetTrigger("Die");                 
                    }
                    m_PlayerTracked = null;
                    break;
            }
            m_State = NewState;
        }
        public void FindClosestPlayer()
        {
            float l_Min = 100;
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
        public void DisableAfterDie()
        {
            gameObject.SetActive(false);
        }
        public void SetDieState()
        {
            Debug.Log("muere para host");
            ChangeState(TState.DIE);
        }
        public void Die()
        {
            Debug.Log("muere para no host");
            m_SendInfo = false;          
        }
        public void Respawn()
        {
            transform.position=GameController.GetGameController().GetSpawn().position;
            m_AlreadyDead = false;
            m_SendInfo = true;
            ChangeState(TState.FIND_PLAYER);
        }
    }
}

