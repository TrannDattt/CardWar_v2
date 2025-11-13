using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
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

        private async Task ChangeScene(string name)
        {
            _curScene = name;
            SceneManager.LoadScene(_curScene);
            // await SceneManager.LoadSceneAsync(_sceneDict[key].name);
        }

        public async void ChangeScene(EScene key)
        {
            if (!_sceneDict.TryGetValue(key, out string s))
            {
                Debug.LogError($"Scene {key} not exist");
                return;
            }

            _preScene = _curScene;
            await ChangeScene(s);
        }

        public async void BackToPreviousScene()
        {
            if (_preScene == "" || SceneManager.GetSceneByName(_preScene) == null)
            {
                await ChangeScene(_sceneDict[EScene.MainMenu]);
                return;
            }

            await ChangeScene(_preScene);
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