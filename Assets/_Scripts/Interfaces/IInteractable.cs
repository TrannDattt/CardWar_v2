using CardWar.Pointer;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CardWar.Interfaces
{
    public interface IInteractable
    {
        public void OnClicked(PointerInteract pointer);
        public void OnHoverEnter(PointerInteract pointer);
        public void OnHoverExit(PointerInteract pointer);
        public void OnPressed(PointerInteract pointer);
    }
}