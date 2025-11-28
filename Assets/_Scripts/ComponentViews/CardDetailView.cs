using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar_v2.Entities;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class CardDetailView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private enum EDetailType
        {
            Char,
            Skill,
        }

        [Header("General")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Vector2 _offsetToRightBorder;
        private bool _isPointerOnUI;

        [Header("Char Detail")]
        [SerializeField] private RectTransform _charDetail;
        [SerializeField] private TextMeshProUGUI _charName;
        [SerializeField] private Image _charImage;
        [SerializeField] private TextMeshProUGUI _atk;
        [SerializeField] private TextMeshProUGUI _hp;
        [SerializeField] private TextMeshProUGUI _armor;
        [SerializeField] private TextMeshProUGUI _resist;
        [SerializeField] private RectTransform _effectDetailContent;
        [SerializeField] private EffectDetailView _effectDetailPrefab;
        private bool _isShownChar;

        //TODO: Add a section for active effects on char

        [Header("Skill Detail")]
        [SerializeField] private TextMeshProUGUI _skillName;
        [SerializeField] private RectTransform _skillDetail;
        [SerializeField] private Image _skillImage;
        [SerializeField] private TextMeshProUGUI _skillDes;
        private bool _isShownSkill;

        // private string GetSkillDetail(SubSkill subSkill, CharacterCard owner)
        // {
        //     return subSkill.GenerateDescription(owner, false);
        // }
        
        public async Task ShowSkillDetail(SkillCard card)
        {
            // Debug.Log($"2.Show detail of card {card}");
            _skillImage.sprite = card.Image;
            _skillName.SetText(card.Name);
            _skillDes.SetText(card.Des);

            if (_isShownChar) await HideDetailView(EDetailType.Char);
            if (!_isShownSkill) await ShowDetailView(EDetailType.Skill);
            // await ShowDetailView(EDetailType.Skill);
        }

        public async Task ShowCharDetail(CharacterCard card)
        {
            _charImage.sprite = card.Image;
            _charName.SetText(card.Name);
            _atk.SetText($" : {card.CurAtk}");
            _hp.SetText($" : {card.CurHp}");
            _armor.SetText($" : {card.CurArmor}");
            _resist.SetText($" : {card.CurResist}");

            var lastEffects = _effectDetailContent.GetComponentsInChildren<EffectDetailView>().ToList();
            lastEffects.ForEach(e => Destroy(e.gameObject));
            var effects = card.ActiveEffects.Values.ToList();
            effects.ForEach(e =>
            {
                EffectDetailView effectDetail = Instantiate(_effectDetailPrefab, _effectDetailContent);
                effectDetail.ShowDetail(e);
            });

            if (_isShownSkill) await HideDetailView(EDetailType.Skill);
            if (!_isShownChar) await ShowDetailView(EDetailType.Char);
            // await ShowDetailView(EDetailType.Char);
        }

        private async Task ShowDetailView(EDetailType type)
        {
            var view = type == EDetailType.Char ? _charDetail : _skillDetail;
            if (type == EDetailType.Char) _isShownChar = true;
            else _isShownSkill = true;

            var sequence = DOTween.Sequence();

            sequence.Append(view.DOAnchorPos(Vector2.zero, .5f).SetEase(Ease.OutBack, overshoot: 1f));

            await sequence.AsyncWaitForCompletion();
        }

        private async Task HideDetailView(EDetailType type)
        {
            var view = type == EDetailType.Char ? _charDetail : _skillDetail;
            if (type == EDetailType.Char) _isShownChar = false;
            else _isShownSkill = false;
            var viewPos = new Vector2(view.rect.width + _offsetToRightBorder.x * 2, 0);
            // var viewPos = new Vector2(view.rect.width + _offsetToRightBorder.x * 2, view.rect.height + _offsetToRightBorder.y * 2);

            var sequence = DOTween.Sequence();

            sequence.Append(view.DOAnchorPos(viewPos, .5f).SetEase(Ease.InOutQuad));

            await sequence.AsyncWaitForCompletion();
        }

        public async Task HideAllDetailView()
        {
            if (!_isShownChar && !_isShownSkill) return;

            var tasks = new List<Task>
            {
                HideDetailView(EDetailType.Char),
                HideDetailView(EDetailType.Skill)
            };

            await Task.WhenAll(tasks);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOnUI = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerOnUI = true;
        }

        void Start()
        {
            // var charPos = new Vector2(_charDetail.rect.width + _offsetToRightBorder.x * 2, _charDetail.rect.height + _offsetToRightBorder.y * 2);
            // var skillPos = new Vector2(_skillDetail.rect.width + _offsetToRightBorder.x * 2, _skillDetail.rect.height + _offsetToRightBorder.y * 2);
            var charPos = new Vector2(_charDetail.rect.width + _offsetToRightBorder.x * 2, 0);
            var skillPos = new Vector2(_skillDetail.rect.width + _offsetToRightBorder.x * 2, 0);

            _charDetail.anchoredPosition = charPos;
            _skillDetail.anchoredPosition = skillPos;

            _isShownChar = false;
            _isShownSkill = false;
        }

        async void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && (_isShownChar || _isShownSkill) && !_isPointerOnUI)
            {
                await HideAllDetailView();
            }
        }
    }
}

