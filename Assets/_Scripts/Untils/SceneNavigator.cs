using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CardWar.Untils
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

        private Dictionary<EScene, string> _sceneDict = new();

        private string _curScene;
        private string _preScene;

        public UnityEvent<EScene> OnSceneLoaded { get; set; } = new();

        private async Task ChangeScene(string name)
        {
            _curScene = name;
            // SceneManager.LoadScene(_curScene);
            var op = SceneManager.LoadSceneAsync(name);

            while (!op.isDone)
                await Task.Yield();
        }

        public async Task ChangeScene(EScene key)
        {
            if (!_sceneDict.TryGetValue(key, out string s))
            {
                Debug.LogError($"Scene {key} not exist");
                return;
            }

            _preScene = _curScene;
            await ChangeScene(s);
            OnSceneLoaded?.Invoke(key);
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
        }
    }
}