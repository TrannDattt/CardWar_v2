using System.Collections.Generic;
using CardWar.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardWar.Untils
{
    public class OutlineSelector : MonoBehaviour
    {
        [SerializeField] private Canvas _mainCanvas;
        private GraphicRaycaster _raycaster;
        private EventSystem _eventSystem;
        private Camera _mainCam;

        private Transform _highlight;
        private QuickOutline HighlightOutline => _highlight.gameObject.GetComponent<QuickOutline>();
        private Transform _selection;
        private QuickOutline SelectionOutline => _selection.gameObject.GetComponent<QuickOutline>();

        private Transform _uiHighlight;
        private QuickOutline UIHighlightOutline => _uiHighlight.gameObject.GetComponent<QuickOutline>();
        private Transform _uiSelection;
        private QuickOutline UISelectionOutline => _uiSelection.gameObject.GetComponent<QuickOutline>();

        private void CheckHighlightObject()
        {
            var ray = _mainCam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 2000, Color.red);
            if (Physics.Raycast(ray, out var raycastHit, Mathf.Infinity, LayerMask.GetMask("Selector Target")))
            // if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out _raycastHit))
            {
                if (_highlight != raycastHit.transform && _selection != raycastHit.transform)
                {
                    _highlight = raycastHit.transform;
                    Debug.Log($"Highlight: {_highlight.gameObject.name}");
                    if (HighlightOutline == null)
                    {
                        _highlight.gameObject.AddComponent<QuickOutline>();
                    }
                    HighlightOutline.enabled = true;
                    HighlightOutline.OutlineColor = new(.5f, .9f, .2f);
                    HighlightOutline.OutlineWidth = 7;
                }
            }
            else if (_highlight)
            {
                HighlightOutline.enabled = false;
                _highlight = null;
            }
        }

        private void CheckSelectObject()
        {
            if (_highlight && _selection != _highlight)
            {
                if (_selection)
                {
                    SelectionOutline.enabled = false;
                }
                _selection = _highlight;
                Debug.Log($"Select: {_selection.gameObject.name}");
                SelectionOutline.enabled = true;
                _highlight = null;
            }
            else if (!_highlight && _selection)
            {
                SelectionOutline.enabled = false;
                _selection = null;
            }
        }

        void Start()
        {
            _mainCam = Camera.main;
            _raycaster = _mainCanvas.GetComponent<GraphicRaycaster>();
            _eventSystem = EventSystem.current;
        }

        void Update()
        {
            CheckHighlightObject();

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                CheckSelectObject();
            }
        }
    }
}