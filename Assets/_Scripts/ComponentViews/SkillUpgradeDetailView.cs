using CardWar_v2.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class SkillUpgradeDetailView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _details;

        public void UpdateDetail(SkillCard skillCard, bool isShowNextLevel)
        {
            _image.sprite = skillCard.Image;
            _name.SetText(skillCard.Name);
            // var detail = "";
            // foreach (var ss in skillCard.SubSkills)
            // {
            //     detail += $"- {ss.GenerateDescription(skillCard.Owner, isShowNextLevel)}{(ss == skillCard.SubSkills[^1] ? "" : "\n")}";
            // }
            // _details.SetText(detail);
            _details.SetText(skillCard.Des);
        }
    }
}

