using DG.Tweening;
using UnityEngine;

namespace CardWar_v2.Views
{
    public class ButtonSelectOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject _overlayView;
        [SerializeField] private GameObject _overlayContent;

        private Vector3 _contentFixedPos;

        public void MoveView(Vector3 newPos)
        {
            _overlayView.transform.DOMove(newPos, .2f).SetEase(Ease.OutQuad).OnUpdate(() =>
            {
                _overlayContent.transform.position = _contentFixedPos;
            });
        }

        void Start()
        {
            _contentFixedPos = _overlayContent.transform.position;
        }
    }
}

