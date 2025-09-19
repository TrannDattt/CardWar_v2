// using CardWar.Pointer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CardWar.Interfaces
{
    public interface ISelectable
    {
        public void OnClicked();
        public void OnHoverEnter();
        public void OnHoverExit();
        public void OnPressed();
    }

    public abstract class SelectableObject : MonoBehaviour, ISelectable
    {
        protected virtual void Awake()
        {
            gameObject.tag = "Selectable";
        }

        void OnMouseDown()
        {
            OnClicked();
        }

        void OnMouseEnter()
        {
            OnHoverEnter();
        }

        void OnMouseExit()
        {
            OnHoverExit();
        }

        public abstract void OnClicked();
        public abstract void OnHoverEnter();
        public abstract void OnHoverExit();
        public abstract void OnPressed();
    }
}