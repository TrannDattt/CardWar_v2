using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    // [RequireComponent(typeof(Rigidbody))]
    public class SkillCardView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        public SkillCard BaseCard { get; private set; }

        [SerializeField] private Canvas _canvas;
        // [SerializeField] private Material _imageMat;
        [SerializeField] private Image _image;

        public UnityEvent OnCardGrab = new();
        public UnityEvent OnCardDrop = new();
        public UnityEvent<PointerEventData> OnCardClick = new();

        private Camera _mainCam;
        private Vector3 _offset;
        private float _zDistance;
        private bool _isGrab;

        private List<Renderer> _rends = new();
        private HashSet<Material> _mats = new();

        public void SetBaseCard(SkillCard card)
        {
            BaseCard = card;

            // _canvas.worldCamera = Camera.main;
            // _image.sprite = BaseCard.Image;
            // if(BaseCard.Image != null) Debug.Log(BaseCard.Image.name);

            _rends.AddRange(GetComponentsInChildren<MeshRenderer>());
            foreach (var r in _rends)
            {
                foreach (var m in r.materials)
                {
                    _mats.Add(m);
                }
            }

            var graphics = GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                var instancedMat = new Material(g.material);
                g.material = instancedMat;

                _mats.Add(instancedMat);
                // if (instancedMat.Equals(_imageMat))
                // {
                //     _imageMat.SetTexture("_BaseColorMap", BaseCard.Image.texture);
                //     Debug.Log("Change image");
                // }
            }

            _image.material.SetTexture("_BaseColorMap", BaseCard.Image != null ? BaseCard.Image.texture : null);
        }

        private void GrabCard()
        {
            // Debug.Log($"Grab card {name}");
            _isGrab = true;
            _zDistance = _mainCam.WorldToScreenPoint(transform.position).z;
            Vector3 mouseWorld = _mainCam.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, _zDistance)
            );
            _offset = transform.position - mouseWorld;
            OnCardGrab?.Invoke();
        }

        private void DropCard()
        {
            // Debug.Log($"Drop card {name}");
            _isGrab = false;
            OnCardDrop?.Invoke();
        }

        private void DragCard()
        {
            Vector3 mouseWorld = _mainCam.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, _zDistance)
            );

            // var posZ = transform.position.z;
            // var newPos = new Vector3(mouseWorld.x + _offset.x, mouseWorld.y + _offset.y, posZ);
            var newPos = mouseWorld + _offset;
            transform.position = newPos;
        }

        private void SetDissolve(float value)
        {
            foreach (var m in _mats)
                m.SetFloat("_Dissolve", value);
        }

        public async Task DestroySkill(float duration)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DORotate(new Vector3(0, 0, -30), .3f).SetEase(Ease.InOutQuad));
            sequence.Join(DOTween.To(() => 0f, x => SetDissolve(x), 1f, duration).SetEase(Ease.InOutQuad));
            sequence.OnComplete(() =>
            {
                SetDissolve(1f);
                CardFactory.Instance.RecycleCardView(this);
            });

            await sequence.AsyncWaitForCompletion();
        }

        public void RecycleCard()
        {
            OnCardDrop.RemoveAllListeners();
            OnCardGrab.RemoveAllListeners();
            OnCardClick.RemoveAllListeners();

            gameObject.SetActive(false);
            SetDissolve(0);
        }

        void OnMouseDrag()
        {
            // DragCard();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnCardClick?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            DropCard();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            GrabCard();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            // if (_isGrab) DragCard();
        }

        void Start()
        {
            _mainCam = Camera.main;
        }
    }
}

