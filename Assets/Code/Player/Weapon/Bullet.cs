using Project.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Gameplay
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField]
        private NetworkIdentity m_NetworkIdentity;
        [SerializeField]
        private WhoActivatedMe m_WhoActivatedMe;

        public float m_MaxDistance;
        public LayerMask m_ShootLayerMask;
        public float m_Counter;
        float m_CurrentCounter;
        private Vector3 m_Direction;
        private float m_Speed;
        public float m_HeadDamage=210;
        public float m_NoHeadDamage=70;

        public float m_ForceCall = 0.16f;
        private float m_CurrentForceCall = 0;

        private void OnEnable()
        {
            m_CurrentCounter = m_Counter;
        }
        public Vector3 Direction
        {
            set
            {
                m_Direction = value;
            }
        }
        public float Speed
        {
            set
            {
                m_Speed = value;
            }
        }
        void Update()
        {
            transform.Translate(Vector3.forward * m_Speed * Time.deltaTime * NetworkClient.SERVER_UPDATE_TIME);
            m_CurrentForceCall -= Time.deltaTime;

            RaycastHit l_RaycastHit;
            if (Physics.Raycast(transform.position, transform.forward, out l_RaycastHit, m_MaxDistance, m_ShootLayerMask.value))
            {
                if (l_RaycastHit.collider.gameObject.tag == "ZombieHead")
                {
                    //ZombieManager l_Zombie = l_RaycastHit.collider.gameObject.GetComponent<ZombieManager>();
                    //l_Zombie.HitByBullet(44);
                    Collision(l_RaycastHit.collider.gameObject,m_HeadDamage);
                }
                if (l_RaycastHit.collider.gameObject.tag == "ZombieBody")
                {
                    //ZombieManager l_Zombie = l_RaycastHit.collider.gameObject.GetComponent<ZombieManager>();
                    //l_Zombie.HitByBullet(44);
                    Collision(l_RaycastHit.collider.gameObject,m_NoHeadDamage);
                }
                else
                {
                    Collision(l_RaycastHit.collider.gameObject,0);
                }
            }
            m_CurrentCounter -= Time.deltaTime;
            if (m_CurrentCounter <= 0)
            {
                if (m_CurrentForceCall <= 0)
                {
                    m_NetworkIdentity.GetSocket().Emit("collisionDestroy", new JSONObject(JsonUtility.ToJson(new IDData()
                    {
                        id = m_NetworkIdentity.GetID(),
                        zombieID = m_NetworkIdentity.GetID(),
                        damage = 0,
                        x = 0,
                        y = 0,
                        z = 0
                    })));
                    m_CurrentForceCall = m_ForceCall;
                }
            }
        }

        public void Collision(GameObject collision,float Damage)
        {
            if (m_CurrentForceCall <= 0)
            {
                NetworkIdentity l_NetworkIdentity = collision.GetComponentInParent<NetworkIdentity>();
                if (l_NetworkIdentity != null)
                {
                    m_NetworkIdentity.GetSocket().Emit("collisionDestroy", new JSONObject(JsonUtility.ToJson(new IDData()
                    {
                        id = m_NetworkIdentity.GetID(),
                        zombieID= l_NetworkIdentity.GetID(),
                        damage = Damage,
                        x = l_NetworkIdentity.transform.position.x,
                        y = l_NetworkIdentity.transform.position.y,
                        z = l_NetworkIdentity.transform.position.z
                    })));
                }
                m_CurrentForceCall = m_ForceCall;
            }
        }

        public void CreateShootHitParticles(Vector3 Position, Vector3 Normal)
        {
            // GameObject.Instantiate(m_HitCollisionParticlesPrefab, Position, Quaternion.LookRotation(Normal), m_controller.m_DestroyObjects);
            GameObject bullet = CPoolElements.SharedInstance.GetDecal();
            if (bullet != null)
            {
                bullet.transform.position = Position;
                bullet.transform.rotation = Quaternion.LookRotation(Normal);
                bullet.SetActive(true);
            }
        }
    }
}

