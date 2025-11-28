using System;
using System.Collections.Generic;
using System.Linq;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using CardWar_v2.GameControl;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class CharacterListView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rt;
        [SerializeField] private RectTransform _container;
        [SerializeField] private SelectBorderView _iconSelectBorder;
        [SerializeField] private SelectBorderView _iconHoverBorder;
        [SerializeField] private float _offsetY;

        public UnityEvent<CharacterIconView, bool> OnIconClicked { get; set; } = new();

        private List<CharacterCard> CharList => PlayerSessionManager.Instance.CharacterList;
        private List<CharacterIconView> _iconList = new();

        private int _selectAmount;
        private List<CharacterIconView> _selectedIcons = new();
        private List<SelectBorderView> _selectedIconBorders = new();

        private Vector3 _originalPos;
        private bool _isVisible = true;

        public void SetSelectAmount(int amount)
        {
            _selectAmount = amount;
        }

        public void ShowList()
        {
            if (_isVisible) return;

            _isVisible = true;
            // _rt.anchoredPosition = _originalPos;
            transform.position = _originalPos;
        }

        public void HideList()
        {
            if (!_isVisible) return;

            _isVisible = false;
            // _rt.anchoredPosition = _originalPos + new Vector2(0, _offsetY);
            transform.position = _originalPos + new Vector3(0, _offsetY);
        }

        public void ShowCharacterIcons(bool showUnlockOnly, bool canMultiSelect)
        {
            _selectedIcons.Clear();
            _selectAmount = canMultiSelect ? _selectAmount : 1;

            _selectedIconBorders.ForEach(b => b.DeselectUI());

            _iconList.Clear();

            foreach (RectTransform rt in _container)
            {
                if (rt.gameObject.TryGetComponent<CharacterIconView>(out var icon))
                {
                    IconFactory.Instance.RecycleIconView(icon);
                }
            }

            var arrangedCharList = CharList.Where(c => c.IsPlayable).OrderByDescending(c => c.IsUnlocked).ThenBy(c => c.Name);

            foreach (var card in arrangedCharList)
            {
                if (showUnlockOnly && !card.IsUnlocked) break;
                var newIcon = IconFactory.Instance.CreateNewCharIcon(card, false, _container);

                newIcon.OnIconClicked.AddListener(() =>
                {
                    if (!canMultiSelect)
                        DeselectOtherIcons(newIcon);
                    else
                    {
                        if (_selectedIcons.Contains(newIcon))
                        {
                            DeselectIcon(newIcon);
                            OnIconClicked?.Invoke(newIcon, false);
                            return;
                        }

                        if (_selectedIcons.Count == _selectAmount)
                        {
                            Debug.Log("Maximum selected");
                            return;
                        }
                    }

                    SelectIcon(newIcon);
                    OnIconClicked?.Invoke(newIcon, true);
                });

                newIcon.OnPointerEnterIcon.AddListener(() => _iconHoverBorder.SelectUI(newIcon.GetComponent<RectTransform>()));
                newIcon.OnPointerExitIcon.AddListener(() => _iconHoverBorder.DeselectUI());

                _iconList.Add(newIcon);
            }

            if (!canMultiSelect)
                _iconList.FirstOrDefault(i => i.BaseCard.Model != null).OnPointerClick(null);
        }

        private void SelectIcon(CharacterIconView icon)
        {
            if (_selectedIcons.Contains(icon)) return;
            
            // Debug.Log($"Selected icon {icon.BaseCard.Name}");
            _selectedIcons.Add(icon);

            if (_selectedIcons.Count > _selectedIconBorders.Count)
            {
                var newBorder = new SelectBorderView(_iconSelectBorder, _container);
                _selectedIconBorders.Add(newBorder);
                newBorder.Rt.SetSiblingIndex(0);
                // newBorder.Rt.SetSiblingIndex(_container.childCount - 1 - _iconList.Count);
            }
            var selectBorder = _selectedIconBorders.FirstOrDefault(s => !s.IsActive);

            selectBorder.SelectUI(icon.GetComponent<RectTransform>());
        }

        private void DeselectIcon(CharacterIconView icon)
        {
            if (!_selectedIcons.Contains(icon)) return;

            _selectedIcons.Remove(icon);
            _selectedIconBorders.FirstOrDefault(b => b.Target == icon.GetComponent<RectTransform>()).DeselectUI();
        }

        private void DeselectOtherIcons(CharacterIconView selectedIcon)
        {
            _selectedIcons.RemoveAll(i => i.BaseCard != selectedIcon.BaseCard);

            foreach(var b in _selectedIconBorders)
            {
                if (b.Target == selectedIcon.GetComponent<RectTransform>()) continue;
                b.DeselectUI();
            }
        }

        void Start()
        {
            _originalPos = transform.position;
        }
    }
    
    [Serializable]
    public class SelectBorderView
    {
        [field: SerializeField] public RectTransform Rt { get; private set; }
        [SerializeField] private Image _border;
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _defaultColor = Color.clear;

        public bool IsActive { get; private set; }
        public RectTransform Target { get; private set; }

        public SelectBorderView(SelectBorderView prefab, RectTransform parent)
        {
            var inst = UnityEngine.Object.Instantiate(prefab._border, parent);

            Rt = inst.rectTransform;
            _border = inst.GetComponent<Image>();
            _selectedColor = prefab._selectedColor;
            _defaultColor = prefab._defaultColor;

            DeselectUI();
        }

        public void SelectUI(RectTransform rt)
        {
            Rt.anchoredPosition = rt.anchoredPosition;
            _border.color = _selectedColor;
            Target = rt;
            IsActive = true;
        }

        public void DeselectUI()
        {
            _border.color = _defaultColor;
            Target = null;
            IsActive = false;
        }
    }
}

