using UnityEngine;
using UnityEngine.EventSystems;

namespace CardWar.Untils
{
    public class OutlineSelector : MonoBehaviour
    {
        private Transform _highlight;
        private Outline HighlightOutline => _highlight.gameObject.GetComponent<Outline>();
        private Transform _selection;
        private Outline SelectionOutline => _selection.gameObject.GetComponent<Outline>();
        private RaycastHit _raycastHit;

        // void Start()
        // {
        //     if (!_highlight.gameObject.TryGetComponent(out _highlightOutline))
        //     {
        //         _highlightOutline = _highlight.gameObject.AddComponent<Outline>();
        //     }
        // }

        void Update()
        {
            if (_highlight)
            {
                // if (HighlightOutline == null)
                // {
                //     _highlight.gameObject.AddComponent<Outline>();
                // }
                HighlightOutline.enabled = false;
                _highlight = null;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out _raycastHit))
            {
                _highlight = _raycastHit.transform;
                Debug.Log($"Highlight: {_highlight.gameObject.name}");
                if (_highlight.CompareTag("Selectable") && _highlight != _selection)
                {
                    if (HighlightOutline == null)
                    {
                        _highlight.gameObject.AddComponent<Outline>();
                    }
                    HighlightOutline.enabled = true;
                    HighlightOutline.OutlineColor = new(.5f, .9f, .2f);
                    HighlightOutline.OutlineWidth = 7;
                }
                else
                {
                    _highlight = null;
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (_highlight)
                {
                    if (_selection)
                    {
                        SelectionOutline.enabled = false;
                    }
                    _selection = _raycastHit.transform;
                Debug.Log($"Select: {_selection.gameObject.name}");
                    SelectionOutline.enabled = true;
                    _highlight = null;
                }
                else if (_selection)
                {
                    SelectionOutline.enabled = false;
                    _selection = null;
                }
            }
        }
    }
}