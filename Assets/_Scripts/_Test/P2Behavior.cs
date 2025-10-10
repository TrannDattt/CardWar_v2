namespace CardWar.Test
{
    using System.Collections.Generic;
    using CardWar.Datas;
    using CardWar.Entities;
    using CardWar.Factories;
    using CardWar.Views;
    using UnityEngine;
    using UnityEngine.UI;

    public class P2Behavior : MonoBehaviour
    {
        // [SerializeField] private Player _player2;
        [SerializeField] private List<CardData> _player2Deck;
        [SerializeField] private PlayerHandView _player2HandView;
        [SerializeField] private Button _drawCardButton;

        private void Start()
        {
            // if (_player2 == null)
            // {
            //     Debug.LogError("Player 2 is not assigned in the inspector.");
            //     return;
            // }

            // if (_player2HandView == null)
            // {
            //     Debug.LogError("PlayerHandView is not assigned in the inspector.");
            //     return;
            // }

            // if (_drawCardButton == null)
            // {
            //     Debug.LogError("DrawCardButton is not assigned in the inspector.");
            //     return;
            // }

            // _drawCardButton.onClick.AddListener(DrawCardForPlayer2);
        }

        private void DrawCardForPlayer2()
        {
            // var drawnCard = _player2.DrawCard();
            // if (drawnCard != null)
            // {
            //     Debug.Log($"Player 2 drew card: {drawnCard.Name}");
            //     _player2HandView.AddCard(drawnCard);
            // }
            // else
            // {
            //     Debug.Log("Player 2's deck is empty. Cannot draw a card.");
            // }
        }
    }
}