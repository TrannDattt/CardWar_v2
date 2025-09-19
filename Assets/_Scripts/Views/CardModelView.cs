using CardWar.Entities;
using TMPro;
using UnityEngine;

namespace CardWar.Views
{
    public class CardModelView : MonoBehaviour
    {
        public Card BaseCard { get; private set; }

        [field: SerializeField] public GameObject Model { get; private set; }
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRend;
        [SerializeField] private MeshCollider _meshColl;
        [SerializeField] private Animator _animator;
        [SerializeField] private TextMeshProUGUI _atk;
        [SerializeField] private TextMeshProUGUI _hp;

         public void SetBaseCard(Card card)
        {
            if (card == null) Debug.LogWarning("No card was set");

            BaseCard = card;

            _meshFilter.mesh = card.Mesh;
            _meshFilter.mesh.RecalculateBounds();
            _meshColl.sharedMesh = null;
            _meshColl.sharedMesh = _meshFilter.mesh;
            _animator.runtimeAnimatorController = card.AnimController;
            
            card.OnCardUpdated.AddListener(UpdateCardDetail);
            if (card is MonsterCard mCard) mCard.OnTakenDamage.AddListener(UpdateCardDetail);
            if (card is ConstructCard cCard) cCard.OnTakenDamage.AddListener(UpdateCardDetail);

            UpdateCardDetail();
        }

        private void UpdateCardDetail()
        {
            _meshRend.material = BaseCard.Material;

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
    }
}

