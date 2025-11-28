using System;
using System.Collections.Generic;
using UnityEngine;
using static CardWar_v2.ComponentViews.FaceEmotion;

namespace CardWar_v2.ComponentViews
{
    public class CharacterFaceSwapper : MonoBehaviour
    {
        [SerializeField] private List<FaceEmotion> _emotions;
        [field: SerializeField] public List<Material> FaceMatRef { get; private set; }

        private Dictionary<Material, Material> _faceMatDict = new();
        private Dictionary<EEmotion, FaceEmotion> _emotionDict = new();
        private FaceEmotion _curEmotion;

        public void SetFaceMat(Material reference, Material mat) 
        {
            _faceMatDict[reference] = mat;
        }

        public void ChangeEmotion(EEmotion key)
        {
            if (_curEmotion != null && _curEmotion.Key == key)
            {
                // Debug.Log($"Change to same emotion: {key}");
                return;
            }

            if (!_emotionDict.TryGetValue(key, out var faceEmotion) || faceEmotion.Sprites == null)
            {
                // Debug.Log($"Emotion {key}'s sprite of character {gameObject.name} is null");
                return;
            }

            _curEmotion = faceEmotion;
            var sprites = faceEmotion.Sprites;

            for (int i = 0; i < FaceMatRef.Count; i++)
            {
                var faceMat = _faceMatDict[FaceMatRef[i]];
                var tex = sprites[i].texture;

                Vector2 tiling = new(
                    sprites[i].rect.width / tex.width,
                    sprites[i].rect.height / tex.height
                );

                Vector2 offset = new(
                    sprites[i].rect.x / tex.width,
                    sprites[i].rect.y / tex.height
                );

                faceMat.SetVector("_Tiling", tiling);
                faceMat.SetVector("_Offset", offset);
            }
        }

        void Awake()
        {
            _emotions.ForEach(e => _emotionDict[e.Key] = e);
            FaceMatRef.ForEach(m => _faceMatDict[m] = new(m));
        }
    }

    [Serializable]
    public class FaceEmotion
    {
        public enum EEmotion
        {
            Normal,
            Happy,
            Confident,
            Angry,
            Sad,
            Fear,
            Suprise,
            Painful
        }

        public EEmotion Key;
        public List<Sprite> Sprites;
    }
}

