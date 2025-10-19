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

        [SerializeField] private GameObject _modelBase;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private HealthBarView _healthBar;
        [SerializeField] private GameObject _effectBar;

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
            //TODO: Rotate effect bar to face camera
            _effectBar.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 90, 0);
            // _canvas.GetComponent<RectTransform>().rotation = Quaternion.Inverse(transform.rotation) * Quaternion.Euler(0, 90, 0);

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
            card.OnChangeHp.AddListener(UpdateCardDetail);
            card.OnApplyEffect.AddListener((effect) => ApplyEffect(effect));

            _healthBar.SetMaxHp(Hp);
            foreach(Transform c in _effectBar.transform)
            {
                var effect = c.GetComponent<EffectView>();
                EffectViewFactory.Instance.ReturnEffectView(effect);
            }

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

        public void ApplyEffect(SkillEffect effect)
        {
            EffectViewFactory.Instance.CreateEffectView(effect, _effectBar.GetComponent<RectTransform>());
        }

        public async Task DestroyChar(float duration)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => 0f, x => SetDissolve(x), 1f, duration).SetEase(Ease.InOutQuad));
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

