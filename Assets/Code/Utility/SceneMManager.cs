using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Utility
{
    public class SceneMManager : Singleton<SceneMManager>
    {

        private List<LevelLoadingData> m_LevelsLoading;
        private List<string> m_CurrentlyLoadedScenes;

        public override void Awake()
        {
            base.Awake();
            m_LevelsLoading = new List<LevelLoadingData>();
            m_CurrentlyLoadedScenes = new List<string>();
        }
        public void Update()
        {
            for (int i = m_LevelsLoading.Count - 1; i >= 0; i--)
            {
                if (m_LevelsLoading[i] == null)
                {
                    m_LevelsLoading.RemoveAt(i);
                    continue;
                }

                if (m_LevelsLoading[i].m_AsyncOperayion.isDone)
                {
                    m_LevelsLoading[i].m_AsyncOperayion.allowSceneActivation = true; //Needed to make sure the scene while fully loaded gets turned on for the player
                    m_LevelsLoading[i].m_OnLevelLoaded.Invoke(m_LevelsLoading[i].m_SceneName);
                    m_CurrentlyLoadedScenes.Add(m_LevelsLoading[i].m_SceneName);
                    m_LevelsLoading.RemoveAt(i);
                    //Hide your loading screen here
                    //ApplicationManager.Instance.HideLoadingScreen();
                }
            }
        }
        public void LoadLevel(string LevelName, Action<string> OnLevelLoaded, bool IsShowingLoadingScreen = false)
        {
            bool value = m_CurrentlyLoadedScenes.Any(x => x == LevelName);

            if (value)
            {
                Debug.LogFormat("Current level ({0}) is already loaded into the game.", LevelName);
                return;
            }

            LevelLoadingData m_LevelLoadingData = new LevelLoadingData();
            m_LevelLoadingData.m_AsyncOperayion = SceneManager.LoadSceneAsync(LevelName, LoadSceneMode.Additive);
            m_LevelLoadingData.m_SceneName = LevelName;
            m_LevelLoadingData.m_OnLevelLoaded = OnLevelLoaded;
            m_LevelsLoading.Add(m_LevelLoadingData);

            if (IsShowingLoadingScreen)
            {
                //Turn on your loading screen here
                //ApplicationManager.Instance.ShowLoadingScreen();
            }
        }

        public void UnLoadLevel(string LevelName)
        {
            foreach (string l_Item in m_CurrentlyLoadedScenes)
            {
                if (l_Item == LevelName)
                {
                    SceneManager.UnloadSceneAsync(LevelName);
                    m_CurrentlyLoadedScenes.Remove(l_Item);
                    return;
                }
            }

            Debug.LogErrorFormat("Failed to unload level ({0}), most likely was never loaded to begin with or was already unloaded.", LevelName);
        }
    }

    [Serializable]
    public class LevelLoadingData
    {
        public AsyncOperation m_AsyncOperayion;
        public string m_SceneName;
        public Action<string> m_OnLevelLoaded;
    }

    public static class SceneList
    {
        public const string MAIN_MENU = "MainMenu";
        public const string LEVEL = "Level";
        public const string ONLINE = "Online";
    }
}