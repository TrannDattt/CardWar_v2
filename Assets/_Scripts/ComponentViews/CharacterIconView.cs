using CardWar_v2.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class CharacterIconView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _level;
        [SerializeField] private Image _selectBorder;

        private bool _isSelected;

        public UnityEvent OnIconClicked = new();

        public CharacterCard BaseCard { get; private set; }

        public void SetBaseCard(CharacterCard card)
        {
            BaseCard = card;

            _icon.sprite = card.Image;
            _level.SetText($"Lv.{card.Level}");

            if (!card.IsUnlocked)
            {
                _icon.color = AdjustColorValue(_icon.color, .5f);
            }
            else
            {
                _icon.color = AdjustColorValue(_icon.color, 1f);
            }

            UnselectIcon();
            OnIconClicked.RemoveAllListeners();
        }

        private Color AdjustColorValue(Color color, float newValue)
        {
            var hsvColor = color;
            Color.RGBToHSV(hsvColor, out float h, out float s, out float v);
            return Color.HSVToRGB(h, s, Mathf.Clamp01(newValue));
        }

        private Color AdjustColorAlpha(Color color, float newAlpha)
        {
            return new(color.r, color.g, color.b, Mathf.Clamp01(newAlpha));
        }

        public void SelectIcon()
        {
            _isSelected = true;
            _selectBorder.color = AdjustColorAlpha(_selectBorder.color, 1);
        }

        public void UnselectIcon()
        {
            _isSelected = false;
            _selectBorder.color = AdjustColorAlpha(_selectBorder.color, 0);
        }

        public void HoverIcon()
        {   
            _selectBorder.color = AdjustColorAlpha(_selectBorder.color, .5f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnIconClicked?.Invoke();

            if (_isSelected) UnselectIcon();
            else SelectIcon();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HoverIcon();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isSelected) UnselectIcon();
            else SelectIcon();
        }
    }
}

