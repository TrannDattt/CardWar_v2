using UnityEngine;

namespace CardWar_v2.ComponentViews
{
    public class ShopShelfView : MonoBehaviour
    {
        [SerializeField] private RectTransform _container;
        [field: SerializeField] public int Size { get; private set; }
    }
}

