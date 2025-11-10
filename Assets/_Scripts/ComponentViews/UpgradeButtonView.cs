using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    [Serializable]
    public class UpgradeButtonView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _goldCost;
        [SerializeField] private TextMeshProUGUI _gemCost;

        [SerializeField] private Button _levelUpBtn;

        public void UpdateCostInfo(int goldCost, int gemCost)
        {
            _goldCost.gameObject.SetActive(goldCost != 0);
            _goldCost.SetText(goldCost.ToString());

            _gemCost.gameObject.SetActive(gemCost != 0);
            _gemCost.SetText(gemCost.ToString());
        }

        public void AddListener(Action callback = null)
        {
            _levelUpBtn.onClick.RemoveAllListeners();
            Debug.Log("Add listener to level up button");
            _levelUpBtn.onClick.AddListener(() => callback());
        }
    }
}

