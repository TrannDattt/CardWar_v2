using TMPro;
using UnityEngine;
using static CardWar_v2.Entities.CharacterCard;

namespace CardWar_v2.ComponentViews
{
    public class StatView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private TextMeshProUGUI _level;
        [SerializeField] private TextMeshProUGUI _hp;
        [SerializeField] private TextMeshProUGUI _atk;
        [SerializeField] private TextMeshProUGUI _amr;
        [SerializeField] private TextMeshProUGUI _res;

        public void UpdateView(int level, CharStat stat, bool isMaxLevel)
        {
            // if (canLevelUp)
            // {
            //     HideView();
            //     return;
            // }

            // ShowView();
            _level.SetText($"Lv. {(!isMaxLevel ? level : "MAX")}");
            _hp.SetText($"Hp: {stat.Hp}");
            _atk.SetText($"Atk: {stat.Atk}");
            _amr.SetText($"Amr: {stat.Armor}");
            _res.SetText($"Res: {stat.Resist}");
        }

        public void ShowView()
        {
            // _canvasGroup.alpha = 1;
            gameObject.SetActive(true);
        }

        public void HideView()
        {
            // _canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
    }
}

