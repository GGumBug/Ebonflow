using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AutoBattle.UI
{
    public class SellZonePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<bool> OnHoverChanged;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverChanged?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverChanged?.Invoke(false);
        }
    }
}