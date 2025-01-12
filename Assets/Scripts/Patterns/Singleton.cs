using UnityEngine;

public interface IDonDestroy { }

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();

                if (_instance == null)
                    _instance = CreateInstance();
            }

            return _instance;
        }
    }

    static T CreateInstance()
    {
        var go = new GameObject() { name = $"[{typeof(T).Name}]" };
        var instance = go.AddComponent<T>();
        if (instance.GetComponent<IDonDestroy>() != null)
            DontDestroyOnLoad(go);
        return instance;
    }
}
