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
            BaseCard = card ?? new(null, 1, true);

            SetIcon(card != null ? BaseCard.Image : null);
            _level?.SetText($"Lv.{BaseCard.Level}");

            if (!BaseCard.IsUnlocked && !ignoreLock)
            {
                _icon.color = AdjustColorValue(_icon.color, .5f);
            }
            else
            {
                _icon.color = AdjustColorValue(_icon.color, 1f);
            }

            BaseCard?.OnCardUnlock.AddListener(() => _icon.color = AdjustColorValue(_icon.color, 1f));

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
    }
}

