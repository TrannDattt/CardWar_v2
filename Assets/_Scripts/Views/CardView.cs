using CardWar.Datas;
using CardWar.Entities;
using CardWar.GameControl;
using CardWar.GameViews;
using CardWar.Interfaces;
using CardWar.Pointer;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardWar.Views
{
    public class CardView : MonoBehaviour, IInteractable, IPointerClickHandler
    {
        public Card BaseCard { get; private set; } = new();

        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _skillDetail;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _atk;
        [SerializeField] private TextMeshProUGUI _hp;

        public UnityEvent OnCardLeftClicked;
        public UnityEvent OnCardRightClicked;

        public void SetBaseCard(Card card)
        {
            if (card == null) Debug.LogWarning("No card was set");

            BaseCard = card;
            card.OnCardUpdated.AddListener(UpdateCardDetail);
            if (card is MonsterCard monsterCard) monsterCard.OnTakenDamaged.AddListener(UpdateCardDetail);

            UpdateCardDetail();
        }

        private void UpdateCardDetail()
        {
            _name.text = BaseCard.Name;
            // TODO: Auto generate skill details
            // _skillDetail.text = data.Skills;
            _image.sprite = BaseCard.Image;

            if (BaseCard is MonsterCard monsterCard)
            {
                _atk.text = $"ATK: {monsterCard.Atk}";
                _hp.text = $"HP: {monsterCard.Hp}";
            }
        }

        void Start()
        {
            //TODO: Add event to show detail in CardDetailView
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnCardLeftClicked?.Invoke();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnCardRightClicked?.Invoke();
            }
        }

        public void OnClicked(PointerInteract pointer)
        {
            // Debug.Log($"Clicked on card: {BaseCard.Name}");
        }

        public void OnHoverEnter(PointerInteract pointer)
        {
            // Debug.Log($"Hover entered card: {BaseCard.Name}");
        }

        public void OnHoverExit(PointerInteract pointer)
        {
            // Debug.Log($"Hover exited on card: {BaseCard.Name}");
        }

        public void OnPressed(PointerInteract pointer)
        {
            // switch (GameManager.Instance.CurrentMenu)
            // {
            //     case GameManager.EGameMenu.MainMenu:
            //         return;

            //     case GameManager.EGameMenu.Ingame:
            //         IngameSceneView.Instance.CardDetailView.ShowCardDetail(BaseCard);
            //         return;

            //     default:
            //         return;
            // }
        }
    }
}

