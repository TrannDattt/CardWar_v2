using System.Collections.Generic;
using CardWar_v2.Entities;
using CardWar_v2.SceneViews;
using UnityEngine;

namespace CardWar_v2.ComponentViews
{
    public class SkillInfoView : InfoView
    {
        [SerializeField] private RectTransform _detailContainer;
        [SerializeField] private List<SkillUpgradeDetailView> _skillDetailViews;

        public override void UpdateInfoView(CharacterCard charCard, bool isMaxLevel)
        {
            var skills = charCard.SkillCards;
            for (int i = 0; i < skills.Count; i++)
            {
                _skillDetailViews[i].UpdateDetail(skills[i], !isMaxLevel && charCard.IsUnlocked);
            }
        }
    }
}

