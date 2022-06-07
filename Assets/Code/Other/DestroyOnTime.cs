using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Utility
{
    public class DestroyOnTime : MonoBehaviour
    {
        [Header("Time")]
        public float m_DestroyOnTime = 3.0f;
    
        void Start()
        {
            StartCoroutine(DestroyOnTimeFn());
        }

        public IEnumerator DestroyOnTimeFn()
        {
            yield return new WaitForSeconds(m_DestroyOnTime);
            Destroy(this.gameObject);
        }
    }
}
