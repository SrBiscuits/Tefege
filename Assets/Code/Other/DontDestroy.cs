using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    //Script to put any object in Dont Destroy on Load
    /*private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }*/

    private void Update()
    {
        DontDestroyOnLoad(this.gameObject);
    }

}
