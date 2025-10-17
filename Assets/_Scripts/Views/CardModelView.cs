// using CardWar.Entities;
// using CardWar.Factories;
// using CardWar.Interfaces;
// using TMPro;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.EventSystems;

// namespace CardWar.Views
// {
//     public class CardModelView : MonoBehaviour, ISelectorTarget
//     {
//         public Card BaseCard { get; private set; }

//         [field: SerializeField] public GameObject Model { get; private set; }
//         [SerializeField] private MeshFilter _meshFilter;
//         [SerializeField] private MeshRenderer _meshRend;
//         [SerializeField] private Animator _animator;
//         [SerializeField] private TextMeshProUGUI _atk;
//         [SerializeField] private TextMeshProUGUI _hp;
        
//         public UnityEvent<PointerEventData> OnModelClicked;

//         public void SetBaseCard(Card card)
//         {
//             if (card == null) Debug.LogWarning("No card was set");

//             BaseCard = card;

//             _meshFilter.mesh = card.Mesh;
//             _meshFilter.mesh.RecalculateBounds();
//             _animator.runtimeAnimatorController = card.AnimController;

//             card.OnCardUpdated.AddListener(UpdateCardDetail);
//             if (card is IDamagable damagable)
//             {
//                 damagable.OnTakenDamage.AddListener(UpdateCardDetail);
//                 // damagable.OnDeath.AddListener(DestroyModel);
//             }

//             UpdateCardDetail();
//         }

//         private void UpdateCardDetail()
//         {
//             _meshRend.material = BaseCard.Material;

//             switch (BaseCard)
//             {
//                 case MonsterCard mCard:
//                     _atk.text = $"ATK: {mCard.Atk}";
//                     _hp.text = $"HP: {mCard.Hp}";
//                     break;

//                 case ConstructCard cCard:
//                     _hp.text = $"HP: {cCard.Hp}";
//                     _atk.gameObject.SetActive(false);
//                     break;

//                 case SpellCard sCard:
//                     _atk.gameObject.SetActive(false);
//                     _hp.gameObject.SetActive(false);
//                     break;
//             }
//         }

//         void OnDestroy()
//         {
//             OnModelClicked.RemoveAllListeners();
//         }

//         public void OnPointerClick(PointerEventData eventData)
//         {
//             OnModelClicked?.Invoke(eventData);
//         }
//     }
// }

