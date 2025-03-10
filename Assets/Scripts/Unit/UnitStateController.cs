using UnityEngine;

public class UnitStateController : MonoBehaviour
{
    [SerializeField, ReadOnly] private UnitState _unitState;

    public UnitState State
    {
        get
        {
            return _unitState;
        }

        set
        {
            _unitState = value;

            switch (_unitState)
            {
                case UnitState.Idle:
                    break;
                case UnitState.Walk:
                    break;
                case UnitState.Attack:
                    break;
            }
        }
    }
}
