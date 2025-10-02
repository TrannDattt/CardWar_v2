using CardWar.Entities;
using CardWar.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardWar.Views
{
    public class CardView : MonoBehaviour, ISelectorTarget, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Card BaseCard { get; private set; } = null;

        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _skillDetail;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _atk;
        [SerializeField] private TextMeshProUGUI _hp;
        [SerializeField] private Outline _outline;

        private bool _isSelected;

        public UnityEvent<PointerEventData> OnCardClicked;

        public void SetBaseCard(Card card)
        {
            _isSelected = false;
            _outline.enabled = false;
            if (card == null) Debug.LogWarning("No card was set");

            BaseCard = card;
            card.OnCardUpdated.AddListener(UpdateCardDetail);
            if (card is MonsterCard mCard) mCard.OnTakenDamage.AddListener(UpdateCardDetail);
            if (card is ConstructCard cCard) cCard.OnTakenDamage.AddListener(UpdateCardDetail);

            UpdateCardDetail();
        }

        private void UpdateCardDetail()
        {
            _name.text = BaseCard.Name;
            // TODO: Auto generate skill details
            // _skillDetail.text = data.Skills;
            _image.sprite = BaseCard.Image;

            switch (BaseCard)
            {
                case MonsterCard mCard:
                    _atk.text = $"ATK: {mCard.Atk}";
                    _hp.text = $"HP: {mCard.Hp}";
                    break;

                case ConstructCard cCard:
                    _hp.text = $"HP: {cCard.Hp}";
                    _atk.gameObject.SetActive(false);
                    break;

                case SpellCard sCard:
                    _atk.gameObject.SetActive(false);
                    _hp.gameObject.SetActive(false);
                    break;
            }
        }

        public void SelectCard()
        {
            _outline.enabled = true;
            _isSelected = true;
        }

        public void DeselectCard()
        {
            _outline.enabled = false;
            _isSelected = false;
        }

        void OnDestroy()
        {
            OnCardClicked.RemoveAllListeners();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isSelected)
            {
                SelectCard();
            }
            else
            {
                DeselectCard();
            }
            OnCardClicked?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isSelected) return;
            _outline.enabled = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _outline.enabled = true;
        }
    }
}

