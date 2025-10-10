using System.Collections.Generic;

public class LateUpdateManager : Singleton<LateUpdateManager>
{
    private int _currentIndex;
    private List<ILateUpdateObserver> _observers = new List<ILateUpdateObserver>();
    private List<ILateUpdateObserver> _pendingObservers = new List<ILateUpdateObserver>();

    private void LateUpdate()
    {
        for (_currentIndex = _observers.Count - 1; _currentIndex >= 0; _currentIndex--)
        {
            _observers[_currentIndex].ObservedLateUpdate();
        }

        _observers.AddRange(_pendingObservers);
        _pendingObservers.Clear();
    }

    public void RegisterObserver(ILateUpdateObserver observer)
    {
        _pendingObservers.Add(observer);
    }

    public void UnRegisterObserver(ILateUpdateObserver observer)
    {
        _observers.Remove(observer);
        _currentIndex--;
    }
}
