using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.Views
{
    public class HealthBarView : MonoBehaviour
    {
        [SerializeField] private Image _bar;

        private float _maxHp;

        public void SetMaxHp(float maxHp)
        {
            _maxHp = maxHp;
        }

        public void UpdateBar(float curHp)
        {
            _bar.fillAmount = Mathf.Clamp(curHp / _maxHp, 0, 1);
        }
    }
}

