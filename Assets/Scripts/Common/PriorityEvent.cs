using System.Collections.Generic;
using System;

public sealed class PriorityEvent
{
    private readonly List<(int priority, Action handler)> _handlers = new();

    public void Add(Action handler, int priority = 0)
    {
        _handlers.Add((priority, handler));
        _handlers.Sort((a, b) => b.priority.CompareTo(a.priority)); // 높은 숫자 먼저
    }

    public void Remove(Action handler)
        => _handlers.RemoveAll(h => h.handler == handler);

    public void Invoke()
    {
        foreach (var h in _handlers) h.handler?.Invoke();
    }
}