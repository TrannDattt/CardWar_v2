using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace CardWar_v2.ComponentViews
{
    public class ButtonSelectOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject _overlayView;
        [SerializeField] private GameObject _overlayContent;
        [SerializeField] private GameObject _contentToOverlay;

        public async Task MoveView(RectTransform target)
        {
            Vector3 viewPos = _overlayView.transform.parent.InverseTransformPoint(target.position);

            await _overlayView.GetComponent<RectTransform>().DOAnchorPos(viewPos, 0.2f)
                .SetEase(Ease.OutQuad)
                .OnUpdate(() =>
                {
                    // Vector3 overlayPos = _overlayContent.transform.parent.InverseTransformPoint(_contentFixedPos);
                    Vector3 overlayPos = _overlayContent.transform.parent.InverseTransformPoint(_contentToOverlay.transform.position);
                    _overlayContent.GetComponent<RectTransform>().anchoredPosition = overlayPos;
                })
                .AsyncWaitForCompletion();
        }
    }
}

