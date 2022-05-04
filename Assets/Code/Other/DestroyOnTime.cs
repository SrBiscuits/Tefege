using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnTime : MonoBehaviour
{
    //Destroy any object in a determined time
    
    [Header("Time")]

    public float m_DestroyOnTime = 3.0f;
    
    void Start()
    {
        StartCoroutine(DestroyOnTimeFn());
    }

    void Update()
    {
        StartCoroutine(DestroyOnTimeFn());
    }

    public IEnumerator DestroyOnTimeFn()
    {
        yield return new WaitForSeconds(m_DestroyOnTime);
        gameObject.SetActive(false);
    }
}
