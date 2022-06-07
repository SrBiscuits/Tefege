using Project.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoad : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        GameController.GetGameController().CanStartGame();
    }
}
