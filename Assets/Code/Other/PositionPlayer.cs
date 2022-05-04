using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionPlayer : MonoBehaviour
{
    //Spawn Player at a certain position when entering scene 2

    private GameObject Player;
    public GameObject spawn;
    private GameObject canvas;

   

    private void Awake()
    {
        
        Player = GameObject.Find("Player");
       
        Player.transform.position = spawn.transform.position;
        Player.transform.rotation = spawn.transform.rotation;
        Player.GetComponent<CharacterController>().enabled = true;
    }
    

  
}
