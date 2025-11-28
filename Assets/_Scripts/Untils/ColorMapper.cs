using System;
using System.Collections.Generic;
using CardWar_v2.Enums;
using UnityEngine;

namespace CardWar_v2.Untils
{
    public static class ColorMapper<T> where T : Enum
    {
        public static Dictionary<ESkillEffect, Color> _effectColorDict = new();
        public static Dictionary<EDamageType, Color> _damageColorDict = new();
        
        public static Color GetColor(T key)
        {
            return key switch
            {
                ESkillEffect effect => _effectColorDict[effect],
                EDamageType damage => _damageColorDict[damage],
                _ => Color.clear,
            };
        }
    }
}