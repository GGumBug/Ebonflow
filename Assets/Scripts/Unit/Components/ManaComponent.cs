using System;
using UnityEngine;

/// <summary>유닛의 마나를 관리하는 순수 로직 컴포넌트입니다.</summary>
public class ManaComponent
{
    private bool _isEnabled = true;

    public event Action<int, int> OnChanged;
    public event Action OnEmptied;
    public event Action OnFilled;

    public int Current { get; private set; }
    public int Max { get; private set; }

    public ManaComponent(int max, int initial = 0)
    {
        if (max == -1)
            _isEnabled = false;

        Max = max;
        Current = Mathf.Clamp(initial, 0, Max);
    }

    public bool IsFull() => Current >= Max && _isEnabled;
    public bool IsEmpty() => Current <= 0 && _isEnabled;

    public void SetMax(int max, bool clampCurrent = true)
    {
        Max = Mathf.Max(1, max);
        if (clampCurrent) Current = Mathf.Clamp(Current, 0, Max);
        OnChanged?.Invoke(Current, Max);
    }

    public void ResetTo(int value)
    {
        int prev = Current;
        Current = Mathf.Clamp(value, 0, Max);
        if (Current != prev)
        {
            OnChanged?.Invoke(Current, Max);
            if (IsFull()) OnFilled?.Invoke();
            else if (IsEmpty()) OnEmptied?.Invoke();
        }
    }

    public void Add(int amount)
    {
        if (amount == 0 || !_isEnabled) return;
        int prev = Current;
        Current = Mathf.Clamp(Current + amount, 0, Max);
        if (Current != prev)
        {
            OnChanged?.Invoke(Current, Max);
            if (IsFull()) OnFilled?.Invoke();
            else if (IsEmpty()) OnEmptied?.Invoke();
        }
    }

    public bool TryConsume(int amount)
    {
        if (amount <= 0) return true;
        if (Current < amount) return false;
        Current -= amount;
        OnChanged?.Invoke(Current, Max);
        if (IsEmpty()) OnEmptied?.Invoke();
        return true;
    }

    public void FillToMax()
    {
        if (Current == Max) return;
        Current = Max;
        OnChanged?.Invoke(Current, Max);
        OnFilled?.Invoke();
    }
}