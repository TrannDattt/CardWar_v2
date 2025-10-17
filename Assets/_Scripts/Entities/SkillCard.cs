using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar_v2.Datas;
using UnityEngine;

namespace CardWar_v2.Entities
{
    public class SkillCard
    {
        private SkillCardData _data;

        public CharacterCard Owner { get; private set; }

        public Sprite Image => _data.Image;
        public List<SubSkill> SubSkills => _data.Skill;

        public SkillCard(SkillCardData data, CharacterCard owner)
        {
            _data = data;
            Owner = owner;
        }
    }
}

