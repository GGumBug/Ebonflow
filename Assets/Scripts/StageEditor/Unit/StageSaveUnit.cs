using System;
using UnityEngine;

namespace StageEditor
{
    [RequireComponent(typeof(Collider2D))]
    public class StageSaveUnit : MonoBehaviour
    {
        private Action<StageSaveUnit> _requestRemoveStageSaveUnit;
        private Func<Vector2> _requestMousePosition;

        public int UnitID       { get; private set; }
        public int StarLevel    { get; private set; }

        public void Setup(int unitID, int starLevel, Action<StageSaveUnit> requestRemoveStageSaveUnit, Func<Vector2> requestMousePosition)
        {
            UnitID = unitID;
            StarLevel = starLevel;
            _requestRemoveStageSaveUnit = requestRemoveStageSaveUnit;
            _requestMousePosition = requestMousePosition;
        }

        private void OnMouseDown()
        {

        }

        private void OnMouseDrag()
        {
            Vector3 mouseWorld = GetMouseWorldPosition();
            transform.position = mouseWorld;
        }

        private void OnMouseUp()
        {
            Vector3 mouseUpPos = transform.position;
            Vector2Int mouseUpPosInt = new Vector2Int(Mathf.RoundToInt(mouseUpPos.x), Mathf.RoundToInt(mouseUpPos.y));
            transform.position = new Vector3(mouseUpPosInt.x, mouseUpPosInt.y, 0.0f);
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 screenPos = Camera.main.ScreenToWorldPoint(_requestMousePosition.Invoke());
            screenPos.z = 0.0f;
            return screenPos;
        }

        public StageEditorUnitInfo GetStageEditorUnitInfo()
        {
            StageEditorUnitInfo info = new StageEditorUnitInfo(
                UnitID,
                StarLevel,
                (int)transform.position.x,
                (int)transform.position.y
                );

            return info;
        }

        public void Destroyed()
        {
            _requestRemoveStageSaveUnit.Invoke(this);
            Destroy(gameObject);
        }
    }
}