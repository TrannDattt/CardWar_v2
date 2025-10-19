// using System.Collections.Generic;
// using System.Threading.Tasks;
// using CardWar_v2.Entities;
// using CardWar_v2.Factories;
// using UnityEngine;

// namespace CardWar_v2.Views
// {
//     public class EffectManager : MonoBehaviour
//     {
//         private List<SkillEffect> _activeEffects = new();

//         public void ClearAllEffects()
//         {
//             foreach (var effect in _activeEffects)
//             {
//                 // EffectViewFactory.Instance.ReturnEffectView(effect);
//             }

//             _activeEffects.Clear();
//         }

//         public async Task ApplyEffects()
//         {
//             foreach (var effect in _activeEffects)
//             {
//                 await effect.ApplyEffect();
//             }
//         }
//     }
// }

