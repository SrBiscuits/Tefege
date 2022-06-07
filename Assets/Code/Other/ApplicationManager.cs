using System.Collections;
using System.Collections.Generic;
using Project.Utility;
using UnityEngine;

namespace Project.Managers
{
    public class ApplicationManager : MonoBehaviour
    {
        //Esto seria para poner una intro de video
        public void Start()
        {
            SceneMManager.Instance.LoadLevel(SceneList.MAIN_MENU, (l_LevelName) => { });
        }
    }
}
