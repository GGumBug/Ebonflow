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

    public bool IsIdle() => State == UnitState.Idle;
    public bool IsWalk() => State == UnitState.Walk;
    public bool IsAttack() => State == UnitState.Attack;
}
