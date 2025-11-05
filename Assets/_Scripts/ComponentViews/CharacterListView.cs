using System;
using System.Collections.Generic;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using CardWar_v2.GameControl;
using DG.Tweening;
using UnityEngine;

namespace CardWar_v2.ComponentViews
{
    public class CharacterListView : MonoBehaviour
    {
        [SerializeField] private RectTransform _container;

        private List<CharacterCard> CharList => PlayerSessionManager.Instance.CharacterList;
        private List<CharacterIconView> _iconList = new();

        public void ShowCharacterIcons(bool showUnlockOnly, Action<CharacterIconView> callback = null)
        {
            _iconList.Clear();
            foreach (RectTransform rt in _container)
            {
                if (rt.gameObject.TryGetComponent<CharacterIconView>(out var icon))
                {
                    CharacterIconFactory.Instance.RecycleIconView(icon);
                }
            }

            foreach (var card in CharList)
            {
                if (showUnlockOnly && !card.IsUnlocked) continue;
                var newIcon = CharacterIconFactory.Instance.CreateNewIcon(card, _container);
                newIcon.OnIconClicked.AddListener(() =>
                {
                    UnselectOtherIcons(newIcon);
                    callback(newIcon);
                });
                _iconList.Add(newIcon);
            }

            _iconList[0].SelectIcon();
        }

        private void UnselectOtherIcons(CharacterIconView selectedIcon)
        {
            foreach(var i in _iconList)
            {
                if (i.BaseCard == selectedIcon.BaseCard) continue;
                i.UnselectIcon();
            }
        }
    }
}

