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
using static CardWar_v2.ComponentViews.FXPlayer;

namespace CardWar_v2.ComponentViews
{
    public class CharacterModelView : MonoBehaviour, IPointerClickHandler
    {
        public CharacterCard BaseCard;

        [SerializeField] private GameObject _modelBase;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private FillBarView _healthBar;
        [SerializeField] private GameObject _effectBar;

        private Animator _animator;

        private List<Renderer> _rends = new();
        private HashSet<Material> _mats = new();
        private CharacterFaceSwapper _faceSwapper;
        private List<FXPlayer> _fxPlayers;

        public float Hp => BaseCard.CurHp;
        public float Atk => BaseCard.CurAtk;
        public EPlayerTarget Side { get; private set; } = EPlayerTarget.Ally;

        public UnityEvent<PointerEventData> OnModelClicked;

        #region Components
        public bool CompareMaterial(Material mat, Material reference)
        {
            return mat.shader == reference.shader && mat.name.Contains(reference.name);
        }

        public void PlayFX(EFXType key)
        {
            _fxPlayers.ForEach(player => player.PlayFXByKey(key));
        }
        #endregion

        #region Card Detail
        public void SetBaseCard(CharacterCard card, EPlayerTarget side)
        {
            if (card == null) Debug.LogWarning("No card was set");

            BaseCard = card;
            Side = side;

            var model = Instantiate(BaseCard.Model, _modelBase.transform);
            // _canvas.GetComponent<RectTransform>().rotation = Quaternion.Inverse(transform.rotation) * Quaternion.Euler(0, 90, 0);

            _animator = model.GetComponent<Animator>();
            _animator.runtimeAnimatorController = card.AnimController;

            _faceSwapper = model.GetComponent<CharacterFaceSwapper>();
            _fxPlayers = GetComponentsInChildren<FXPlayer>().ToList();

            _rends.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());
            _rends.AddRange(GetComponentsInChildren<MeshRenderer>());
            foreach (var r in _rends)
            {
                if (r == null) continue;
                foreach (var m in r.materials)
                {
                    _mats.Add(m);
                    if (_faceSwapper == null) continue;

                    foreach (var matRef in _faceSwapper.FaceMatRef)
                    {
                        if(CompareMaterial(m, matRef))
                        {
                            _faceSwapper.SetFaceMat(matRef, m);
                        }}
                }
            }

            card.OnTakingDamage.AddListener(source => 
            {
                switch (source)
                { 
                    case EFXType.Hit: 
                        PlayFX(EFXType.Hit);
                        break;
                    default: break;
                };
            });
            card.OnChangeHp.AddListener(UpdateCardDetail);
            card.OnApplyEffect.AddListener((effect) => ApplyEffect(effect));

            // _healthBar?.SetMaxValue(Hp);
            if (_effectBar != null)
            {
                _effectBar.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 90, 0);
                foreach (Transform c in _effectBar.transform)
                {
                    var effect = c.GetComponent<EffectView>();
                    EffectViewFactory.Instance.ReturnEffectView(effect);
                }
            }

            UpdateCardDetail();
        }

        private async void UpdateCardDetail()
        {
            if (_healthBar == null) return;

            var maxHp = BaseCard.GetStatAtLevel(BaseCard.Level).Hp;
            _healthBar.Initialize(Side);
            await _healthBar.UpdateBarByValue(Hp, maxHp);
            // Debug.Log($"Hp remain: {Hp}");
        }
        #endregion

        #region Do Skill
        public void DoSkillAnim(AnimationClip clip)
        {
            if (clip == null) return;
            // Debug.Log($"Do animation {clip.name}");
            _animator.Play(clip.name);
        }

        public async Task UseSkill(SubSkill subSkill, List<CharacterModelView> targets)
        {
            // Debug.Log($"Doing sub-skill {subSkill.GetType()}");
            DoSkillAnim(subSkill.Clip);
            var tasks = targets.Where(t => t != null).Select(t => subSkill.DoSkill(this, t));
            await Task.Delay((int)(subSkill.DelayToSkill * 1000));
            await Task.WhenAll(tasks);
            if (subSkill.Clip != null) await WaitForAnimationEnd(_animator, subSkill.Clip.name);
            // Debug.Log($"Finished sub-skill {subSkill.GetType()}");
        }

        private async Task WaitForAnimationEnd(Animator animator, string clipName)
        {
            if (animator == null) return;

            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfos[0].clip.name != clipName) return;
            while (clipInfos.Length == 0)
            {
                await Task.Yield();
                clipInfos = animator.GetCurrentAnimatorClipInfo(0);
            }

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            while (stateInfo.normalizedTime < 1f)
            {
                await Task.Yield();
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }
        }

        public void ApplyEffect(SkillEffect effect)
        {
            EffectViewFactory.Instance.CreateEffectView(effect, _effectBar.GetComponent<RectTransform>());
        }
        #endregion

        #region Char Die
        private void SetDissolve(float value)
        {
            foreach (var m in _mats)
                m.SetFloat("_Dissolve", value);
        }

        public async Task DestroyChar(float duration)
        {
            _animator.Play("Die");
            var sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => 0f, x => SetDissolve(x), 1f, duration).SetEase(Ease.InOutQuad));
            sequence.OnComplete(() =>
            {
                SetDissolve(1f);
                // BaseCard.Die();
                CardFactory.Instance.RecycleCardModel(this);
            });

            await sequence.AsyncWaitForCompletion();
        }
        #endregion

        void OnDestroy()
        {
            OnModelClicked.RemoveAllListeners();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"Clicked to character {BaseCard.Name}");
            OnModelClicked?.Invoke(eventData);
        }
    }
}

