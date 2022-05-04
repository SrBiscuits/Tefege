using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    //Class that is used to make any object look at the player
    private GameObject Player;
    private void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }
    
    void Update()
    {
        transform.LookAt(Player.transform, Vector3.up);
    }
}
