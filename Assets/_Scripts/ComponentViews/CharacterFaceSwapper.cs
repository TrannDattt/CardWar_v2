using System;
using System.Collections.Generic;
using UnityEngine;
using static CardWar_v2.ComponentViews.FaceEmotion;

namespace CardWar_v2.ComponentViews
{
    public class CharacterFaceSwapper : MonoBehaviour
    {
        [SerializeField] List<FaceEmotion> _emotions;
        [SerializeField] Material _faceMat;

        private Dictionary<EEmotion, FaceEmotion> _emotionDict = new();
        private FaceEmotion _curEmotion;

        public void ChangeEmotion(EEmotion key)
        {
            if (_curEmotion != null && _curEmotion.Key == key)
                return;

            if (!_emotionDict.TryGetValue(key, out var faceEmotion) || faceEmotion.Sprite == null)
                return;

            _curEmotion = faceEmotion;
            var sprite = faceEmotion.Sprite;
            var tex = sprite.texture;

            Vector2 tiling = new(
                sprite.rect.width / tex.width,
                sprite.rect.height / tex.height
            );

            Vector2 offset = new(
                sprite.rect.x / tex.width,
                sprite.rect.y / tex.height
            );

            _faceMat.SetVector("_Tiling", tiling);
            _faceMat.SetVector("_Offset", offset);
        }

        void Start()
        {
            _emotions.ForEach(e => _emotionDict[e.Key] = e);
            ChangeEmotion(EEmotion.Normal);
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
        public Sprite Sprite;
    }
}

