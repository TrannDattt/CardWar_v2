using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar_v2.GameControl;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CardWar_v2.Untils
{
    public enum EScene
    {
        MainMenu,
        Arena,
        Ingame,
        Campaign,
    }

    public class SceneNavigator : Singleton<SceneNavigator>
    {
        [Serializable]
        struct GameScene
        {
            public EScene Key;
            public string Name;
        }

        [SerializeField] private List<GameScene> _scenes;
        [SerializeField] private CanvasGroup _loadingScreen;

        private Dictionary<EScene, string> _sceneDict = new();

        private string _curScene;
        private string _preScene;

        public UnityEvent<EScene> OnSceneLoaded { get; set; } = new();

        private void StartLoading()
        {
            _loadingScreen.alpha = 1;
            _loadingScreen.blocksRaycasts = true;
        }

        private void StopLoading()
        {
            _loadingScreen.alpha = 0;
            _loadingScreen.blocksRaycasts = false;
        }

        private async Task ChangeScene(string name)
        {
            StartLoading();

            // var unloadTask = SceneManager.UnloadSceneAsync(_curScene);
            // while (!unloadTask.isDone)
            //     await Task.Yield();

            _curScene = name;
            
            var loadTask = SceneManager.LoadSceneAsync(_curScene);
            while (!loadTask.isDone)
                await Task.Yield();

            await Task.Yield();

            StopLoading();
        }

        public async Task ChangeScene(EScene key, Action callback = null)
        {
            GameplayManager.Instance.ResumeGame();
            if (!_sceneDict.TryGetValue(key, out string s))
            {
                Debug.LogError($"Scene {key} not exist");
                return;
            }

            _preScene = _curScene;
            await ChangeScene(s);
            OnSceneLoaded?.Invoke(key);
            callback?.Invoke();
        }

        public async Task BackToPreviousScene()
        {
            if (_preScene == "" || SceneManager.GetSceneByName(_preScene) == null)
            {
                await ChangeScene(_sceneDict[EScene.MainMenu]);
                OnSceneLoaded?.Invoke(EScene.MainMenu);
                return;
            }

            await ChangeScene(_preScene);
            var sceneKey = _sceneDict.FirstOrDefault(s => s.Value == _preScene).Key;
            OnSceneLoaded?.Invoke(sceneKey);
        }

        protected override void Awake()
        {
            base.Awake();
            _scenes.ForEach(s => _sceneDict[s.Key] = s.Name);
        }

        void Start()
        {
            _curScene = SceneManager.GetActiveScene().name;
            _preScene = "";
            StopLoading();
        }
    }
}