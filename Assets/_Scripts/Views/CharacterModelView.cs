using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar.Interfaces;
using CardWar_v2.Datas;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using CardWar_v2.Factories;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CardWar_v2.Views
{
    public class CharacterModelView : MonoBehaviour
    {
        public CharacterCard BaseCard;

        [SerializeField] private HealthBarView _healthBar;
        [SerializeField] private GameObject _modelBase;

        private Animator _animator;

        private List<Renderer> _rends = new();
        private HashSet<Material> _mats = new();

        public float Hp => BaseCard.Hp;
        public float Atk => BaseCard.Atk;

        public UnityEvent<PointerEventData> OnModelClicked;

        public void SetBaseCard(CharacterCard card)
        {
            if (card == null) Debug.LogWarning("No card was set");

            BaseCard = card;

            Instantiate(BaseCard.Model, _modelBase.transform);
            _animator = _modelBase.GetComponentInChildren<Animator>();
            _animator.runtimeAnimatorController = card.AnimController;

            _rends.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());
            _rends.AddRange(GetComponentsInChildren<MeshRenderer>());
            foreach (var r in _rends)
            {
                foreach(var m in r.materials)
                {
                    _mats.Add(m);
                }
            }

            card.OnCardUpdated.AddListener(UpdateCardDetail);
            card.OnTakenDamage.AddListener(UpdateCardDetail);
            // card.OnAttack.AddListener((t) => DoAttack(t));

            _healthBar.SetMaxHp(Hp);

            UpdateCardDetail();
        }

        private void UpdateCardDetail()
        {
            _healthBar.UpdateBar(Hp);
            // Debug.Log($"Hp remain: {Hp}");
        }

        public void DoSkillAnim(AnimationClip clip)
        {
            //TODO: Do animation: Charge toward target for melee or summon projectile for range
            //      Do damage to target
            if (clip == null) return;
            _animator.Play(clip.name);
        }

        public async Task UseSkill(SubSkill subSkill, List<CharacterModelView> targets)
        {
            DoSkillAnim(subSkill.Clip);
            await Task.Delay((int)(subSkill.DelayToSkill * 1000));
            var tasks = targets.Where(t => t != null).Select(t => subSkill.DoSkill(this, t));
            await Task.WhenAll(tasks);
        }

        private void SetDissolve(float value)
        {
            foreach (var m in _mats)
                m.SetFloat("_Dissolve", value);
        }

        public async Task DestroyChar(float duration)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => 0f, x => SetDissolve(x), 1f, duration)).SetEase(Ease.InOutQuad);
            sequence.OnComplete(() =>
            {
                SetDissolve(1f);
                CardFactory.Instance.RecycleCardModel(this);
            });

            await sequence.AsyncWaitForCompletion();
        }

        public void RecycleModel()
        {
            foreach (Transform t in _modelBase.transform)
            {
                Destroy(t.gameObject);
            }
            OnModelClicked.RemoveAllListeners();

            gameObject.SetActive(false);
            SetDissolve(0);
        }

        void OnDestroy()
        {
            OnModelClicked.RemoveAllListeners();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnModelClicked?.Invoke(eventData);
        }
    }
}

