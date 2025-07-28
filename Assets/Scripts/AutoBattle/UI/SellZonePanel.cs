using UnityEngine;
using UnityEngine.EventSystems;

namespace AutoBattle.UI
{
    public class SellZonePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("Enter SellZone");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("Exit SellZone");
        }
    }
}