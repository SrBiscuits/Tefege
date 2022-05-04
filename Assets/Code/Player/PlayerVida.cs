using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerVida : MonoBehaviour
{
    [Header("Player Attributes")]

    public float health = 100.0f;
    public float armor = 100.0f;
    public float currentHealth;
    public float currentArmor;
    public bool dead = false;
  
    private GameController m_GameController;


    private void Awake()
    {
        m_GameController = GameObject.FindObjectOfType<GameController>();
    }

    void Start()
    {
        currentArmor = armor;
        currentHealth = health; 
    }

    public void Damage(int damage)
    {
        int damage75 = (int)(damage * 0.75f);
        int damage25 = (int)(damage * 0.25f);
        if (currentArmor > 0)
        {
            currentArmor -= damage75;
            currentHealth -= damage25;
        }
        else
        {
            currentHealth -= damage;
        }
    }   
}
