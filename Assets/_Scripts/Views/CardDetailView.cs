// using CardWar.Entities;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// namespace CardWar.Views
// {
//     public class CardDetailView : MonoBehaviour
//     {
//         [SerializeField] private CanvasGroup _canvasGroup;
//         [SerializeField] private Image _background;
//         [SerializeField] private Image _cardImage;
//         [SerializeField] private TextMeshProUGUI _cardName;
//         [SerializeField] private TextMeshProUGUI _cardAtk;
//         [SerializeField] private TextMeshProUGUI _cardHp;
//         [SerializeField] private TextMeshProUGUI _cardSkillDetail;

//         public void ShowCardDetail(Card card)
//         {
//             _cardImage.sprite = card.Image;
//             _cardName.text = card.Name;
//             if (card is MonsterCard monsterCard)
//             {
//                 _cardAtk.text = $"ATK: {monsterCard.Atk}";
//                 _cardHp.text = $"HP: {monsterCard.Hp}";
//             }
//             _cardSkillDetail.text = "Skill details here"; // TODO: Populate skill details
//             // _background.enabled = true;
//             _canvasGroup.alpha = 1;
//         }

//         //// Use in button
//         public void HideCardDetail()
//         {
//             // _background.enabled = false;
//             _canvasGroup.alpha = 0;
//         }
//     }
// }

