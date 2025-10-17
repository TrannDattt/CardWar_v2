// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using CardWar.Entities;
// using CardWar.Factories;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;

// namespace CardWar.Views
// {
//     public class CardSelectorView : MonoBehaviour
//     {
//         [SerializeField] private CanvasGroup _canvasGroup;
//         [SerializeField] private RectTransform _selectorContent;
//         [SerializeField] private Button _confirmBtn;

//         private List<Card> _selectedCards = new();
//         private Func<List<Card>, bool> _checkFunc;

//         private TaskCompletionSource<List<Card>> _tcs;

//         private readonly Dictionary<CardView, UnityAction<PointerEventData>> _cardClickListeners = new();

//         public Task<List<Card>> ShowCardToSelect(List<Card> cards, int amountRequired = 1, Func<List<Card>, bool> checkFunc = null)
//         {
//             _checkFunc = checkFunc ?? (cardsList => DefaultCheckFunc(amountRequired));
//             _selectedCards.Clear();
//             _tcs = new();
//             ToggleConfirmBtn();

//             foreach (var c in cards)
//             {
//                 var cardView = CardFactory.Instance.CreateCardView(c, parent: _selectorContent);
//                 void listener(PointerEventData _)
//                 {
//                     HandleCardViewSelected(cardView);
//                     ToggleConfirmBtn();
//                 }

//                 _cardClickListeners[cardView] = listener;
//                 cardView.OnCardClicked.AddListener(listener);
//             }

//             gameObject.SetActive(true);

//             return _tcs.Task;
//         }
        
//         private bool DefaultCheckFunc(int amountRequired) => _selectedCards.Count == amountRequired;

//         private void ToggleConfirmBtn()
//         {
//             if (_checkFunc == null) return;

//             if (!_checkFunc(_selectedCards))
//             {
//                 _confirmBtn.interactable = false;
//                 return;
//             }
//             _confirmBtn.interactable = true;
//         }

//         public void OnConfirmSelection()
//         {
//             if (_checkFunc != null && !_checkFunc(_selectedCards))
//             {
//                 Debug.LogWarning("Selection not met the requirement");
//                 return;
//             }

//             _tcs?.TrySetResult(_selectedCards);
//             HideCardSelector();
//         }

//         public void OnCancelSelection()
//         {
//             _tcs?.TrySetResult(null);
//             HideCardSelector();
//         }

//         public void HideCardSelector()
//         {
//             var children = _selectorContent.Cast<Transform>().ToList();

//             foreach (Transform child in children)
//             {
//                 if (child.TryGetComponent<CardView>(out var cardView))
//                 {
//                     if (_cardClickListeners.TryGetValue(cardView, out var listener))
//                     {
//                         cardView.OnCardClicked.RemoveListener(listener);
//                         _cardClickListeners.Remove(cardView);
//                     }
//                     CardFactory.Instance.RecycleCardView(cardView);
//                 }
//             }

//             _selectedCards.Clear();
//             gameObject.SetActive(false);
//         }

//         private void HandleCardViewSelected(CardView cardView)
//         {
//             if (_selectedCards.Contains(cardView.BaseCard))
//             {
//                 _selectedCards.Remove(cardView.BaseCard);
//             }
//             else
//             {
//                 _selectedCards.Add(cardView.BaseCard);
//             }
//         }
//     }
// }

