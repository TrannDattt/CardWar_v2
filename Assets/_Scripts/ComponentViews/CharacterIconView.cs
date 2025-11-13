using CardWar_v2.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class CharacterIconView : IconView
    {
        [SerializeField] private TextMeshProUGUI _level;
        // [SerializeField] private Image _selectBorder;

        // private bool _isSelected;

        public CharacterCard BaseCard { get; private set; }

        public void SetBaseCard(CharacterCard card, bool ignoreLock)
        {
            card ??= new(null, 1, true);
            BaseCard = card;

            SetIcon(card.Image);
            _level?.SetText($"Lv.{card.Level}");

            if (!card.IsUnlocked && !ignoreLock)
            {
                _icon.color = AdjustColorValue(_icon.color, .5f);
            }
            else
            {
                _icon.color = AdjustColorValue(_icon.color, 1f);
            }

            card?.OnCardUnlock.AddListener(() => _icon.color = AdjustColorValue(_icon.color, 1f));

            // UnselectIcon();
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

        // public void SelectIcon()
        // {
        //     _isSelected = true;
        //     _selectBorder.color = AdjustColorAlpha(_selectBorder.color, 1);
        // }

        // public void UnselectIcon()
        // {
        //     _isSelected = false;
        //     _selectBorder.color = AdjustColorAlpha(_selectBorder.color, 0);
        // }

        // public void HoverIcon()
        // {
        //     _selectBorder.color = AdjustColorAlpha(_selectBorder.color, .5f);
        // }

        // public override void OnPointerClick(PointerEventData eventData)
        // {
        //     base.OnPointerClick(eventData);

        //     if (_isSelected) UnselectIcon();
        //     else SelectIcon();
        // }

        // public override void OnPointerEnter(PointerEventData eventData)
        // {
        //     base.OnPointerEnter(eventData);

        //     HoverIcon();
        // }

        // public override void OnPointerExit(PointerEventData eventData)
        // {
        //     base.OnPointerExit(eventData);

        //     if (!_isSelected) UnselectIcon();
        //     else SelectIcon();
        // }
    }
}

