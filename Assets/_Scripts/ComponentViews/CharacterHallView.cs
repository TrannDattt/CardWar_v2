using CardWar.Enums;
using CardWar_v2.Datas;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CardWar_v2.ComponentViews
{
    public class CharacterHallView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject _modelBase;
        [SerializeField] private GameObject _charModelHolder;

        [SerializeField] private float _rotateSpeed = 5f;

        private CharacterCard _selectedChar;

        private bool _isPointerOnUI;
        private bool _isRotating;
        private Vector3 _mouseDownPos;
        private Quaternion _baseRotation;

        public void ChangeCharacter(CharacterCard charCard)
        {
            if (_selectedChar == charCard) return;
            _selectedChar = charCard;

            var curModel = _charModelHolder.GetComponentInChildren<CharacterModelView>();
            if (curModel != null)
            {
                CardFactory.Instance.RecycleCardModel(curModel);
            }

            CardFactory.Instance.CreateCharModel(charCard, parent: _charModelHolder.transform);
        }

        private void RotateModel()
        {
            var mouseDelta = Input.mousePosition - _mouseDownPos;
            var rotation = _baseRotation * Quaternion.Euler(0, -mouseDelta.x * _rotateSpeed, 0);
            _modelBase.transform.rotation = rotation;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerOnUI = true;
            // Debug.Log("Pointer entered Character Hall UI");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOnUI = false;
            // Debug.Log("Pointer exited Character Hall UI");
        }

        void Update()
        {
            if (_isPointerOnUI && Input.GetKeyDown(KeyCode.Mouse0))
            {
                _mouseDownPos = Input.mousePosition;
                _baseRotation = _modelBase.transform.rotation;
                _isRotating = true;
            }

            if (_isRotating && Input.GetKey(KeyCode.Mouse0))
            {
                RotateModel();
            }
            
            if(_isRotating && Input.GetKeyUp(KeyCode.Mouse0))
            {
                _isRotating = false;
            }
        }
    }
}

