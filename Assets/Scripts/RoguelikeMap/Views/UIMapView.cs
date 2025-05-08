using UnityEngine;
using UnityEngine.UI;

namespace RoguelikeMap
{
    public class UIMapView : MonoBehaviour
    {
        [SerializeField] private ScrollRect _mapScrollRect;
        [SerializeField] private RectTransform _mapContentRect;
        [SerializeField] private GameObject _nodeViewPrefab;
        [SerializeField] private GameObject _edgeViewPrefab;

        public void RenderMap(MapLayout layout)
        {

        }
    }
}
