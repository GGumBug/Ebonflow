using System.Collections.Generic;

public class UpdateManager : Singleton<UpdateManager>
{
    private int _currentIndex;
    private List<IUpdateObserver> _observers = new List<IUpdateObserver>();
    private List<IUpdateObserver> _pendingObservers = new List<IUpdateObserver>();

    private void Update()
    {
        for (_currentIndex = _observers.Count - 1; _currentIndex >= 0; _currentIndex--)
        {
            _observers[_currentIndex].ObservedUpdate();
        }

        _observers.AddRange(_pendingObservers);
        _pendingObservers.Clear();
    }

    public void RegisterObserver(IUpdateObserver observer)
    {
        _pendingObservers.Add(observer);
    }

    public void UnRegisterObserver(IUpdateObserver observer)
    {
        _observers.Remove(observer);
        _currentIndex--;
    }
}
