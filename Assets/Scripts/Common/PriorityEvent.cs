using System.Collections.Generic;
using System;

public sealed class PriorityEvent
{
    private readonly List<(int priority, Action handler)> _handlers = new();

    public void Add(Action handler, int priority = 0)
    {
        _handlers.Add((priority, handler));
        // 낮은 숫자가 먼저 실행되도록 정렬
        _handlers.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    public void Remove(Action handler)
        => _handlers.RemoveAll(h => h.handler == handler);

    public void Invoke()
    {
        foreach (var h in _handlers)
            h.handler?.Invoke();
    }
}