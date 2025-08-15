using CardWar.Datas;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CardWar.Entities
{
    public class CardView : MonoBehaviour
    {
        private Card _baseCard = new();

        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _skillDetail;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _atk;
        [SerializeField] private TextMeshProUGUI _hp;

        // TEST
        [SerializeField] private CardData _testData;

        void Start()
        {
            SetBaseCard(new MonsterCard(_testData));
        }
        //

        void Awake()
        {
            
        }

        public void SetBaseCard(Card card)
        {
            if (card == null) Debug.LogWarning("No card was set");

            _baseCard = card;
            card.OnCardUpdated.AddListener(UpdateCardDetail);
            if (card is MonsterCard monsterCard) monsterCard.OnTakenDamaged.AddListener(UpdateCardDetail);

            UpdateCardDetail();
        }

        private void UpdateCardDetail()
        {
            if (_baseCard.Data == null) return;
            var data = _baseCard.Data;

            _name.text = data.Name;
            // TODO: Auto generate skill details
            // _skillDetail.text = data.Skills;
            _image.sprite = data.Image;

            if (_baseCard is MonsterCard monsterCard)
            {
                _atk.text = $"ATK: {monsterCard.Atk}";
                _hp.text = $"HP: {monsterCard.Hp}";
            }
        }
    }
}

