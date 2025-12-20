using System.Collections;
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
        
        public IEnumerator ShowSkillDetail(SkillCard card)
        {
            // Debug.Log($"2.Show detail of card {card}");
            _skillImage.sprite = card.Image;
            _skillName.SetText(card.Name);
            _skillDes.SetText(card.Des);

            if (_isShownChar) yield return HideDetailView(EDetailType.Char);
            if (!_isShownSkill) yield return ShowDetailView(EDetailType.Skill);
            // await ShowDetailView(EDetailType.Skill);
        }

        public IEnumerator ShowCharDetail(CharacterCard card)
        {
            _charImage.sprite = card.Image;
            _charName.SetText(card.Name);
            _atk.SetText($" : {(int)card.CurAtk}");
            _hp.SetText($" : {(int)card.CurHp}");
            _armor.SetText($" : {(int)card.CurArmor}");
            _resist.SetText($" : {(int)card.CurResist}");

            var lastEffects = _effectDetailContent.GetComponentsInChildren<EffectDetailView>().ToList();
            lastEffects.ForEach(e => Destroy(e.gameObject));
            var effects = card.ActiveEffects.Values.ToList();
            effects.ForEach(e =>
            {
                if (e.Duration >= 0)
                {
                    EffectDetailView effectDetail = Instantiate(_effectDetailPrefab, _effectDetailContent);
                    effectDetail.ShowDetail(e);
                }
            });

            if (_isShownSkill) yield return HideDetailView(EDetailType.Skill);
            if (!_isShownChar) yield return ShowDetailView(EDetailType.Char);
            // await ShowDetailView(EDetailType.Char);
        }

        private IEnumerator ShowDetailView(EDetailType type)
        {
            var view = type == EDetailType.Char ? _charDetail : _skillDetail;
            if (type == EDetailType.Char) _isShownChar = true;
            else _isShownSkill = true;

            var sequence = DOTween.Sequence();

            sequence.Append(view.DOAnchorPos(Vector2.zero, .5f).SetEase(Ease.OutBack, overshoot: 1f));

            yield return sequence.WaitForCompletion();
        }

        private IEnumerator HideDetailView(EDetailType type)
        {
            var view = type == EDetailType.Char ? _charDetail : _skillDetail;
            if (type == EDetailType.Char) _isShownChar = false;
            else _isShownSkill = false;
            var viewPos = new Vector2(view.rect.width + _offsetToRightBorder.x * 2, 0);
            // var viewPos = new Vector2(view.rect.width + _offsetToRightBorder.x * 2, view.rect.height + _offsetToRightBorder.y * 2);

            var sequence = DOTween.Sequence();

            sequence.Append(view.DOAnchorPos(viewPos, .5f).SetEase(Ease.InOutQuad));

            yield return sequence.WaitForCompletion();
        }

        public IEnumerator HideAllDetailView()
        {
            if (_isShownSkill) yield return HideDetailView(EDetailType.Skill);
            if (_isShownChar) yield return HideDetailView(EDetailType.Char);
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

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && (_isShownChar || _isShownSkill) && !_isPointerOnUI)
            {
                StartCoroutine(HideAllDetailView());
            }
        }
    }
}

