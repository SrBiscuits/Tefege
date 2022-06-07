using Project.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Gameplay
{
    public class ZombieSpawnPoints : MonoBehaviour
    {
        public List<Transform> m_SpawnPoints;

        private void Awake()
        {
            GameController.GetGameController().SetAllZombieSpawnTransform(m_SpawnPoints);
        }
    }
}
