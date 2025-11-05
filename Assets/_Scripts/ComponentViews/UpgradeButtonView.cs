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
            _goldCost.SetText(goldCost.ToString());
            if (gemCost == 0) _gemCost.gameObject.SetActive(false);
            else
            {
                _gemCost.gameObject.SetActive(true);
                _gemCost.SetText(gemCost.ToString());
            }
        }

        public void AddListener(Action callback = null)
        {
            _levelUpBtn.onClick.RemoveAllListeners();
            Debug.Log("Add listener to level up button");
            _levelUpBtn.onClick.AddListener(() => callback());
        }
    }
}

