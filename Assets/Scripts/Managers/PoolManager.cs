using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    #region Pool
    class Pool
    {
        public GameObject Original { get; private set; }
        public Transform Root { get; set; }
        Stack<Poolable> _poolStack = new Stack<Poolable>();

        public void Init(GameObject original, int count = 5)
        {
            Original = original;
            Root = new GameObject().transform;
            Root.name = $"{original.name}_Root";

            for (int i = 0; i < count; i++)
                Push(Create());
        }

        private Poolable Create()
        {
            GameObject go = Instantiate(Original);
            go.name = Original.name;

            Poolable poolable = go.GetComponent<Poolable>();
            if (poolable == null)
            {
                Debug.LogWarning($"[PoolManager] {go.name}에 Poolable 컴포넌트가 없습니다. 자동 추가합니다.");
                poolable = go.AddComponent<Poolable>();
            }

            return poolable;
        }


        public void Push(Poolable poolable)
        {
            if (poolable == null)
                return;

            poolable.ResetState(); // 상태 초기화
            poolable.transform.SetParent(Root);
            _poolStack.Push(poolable);
        }


        public Poolable Pop(Transform parent, Vector3 position = default, Quaternion rotation = default)
        {
            Poolable poolable;
            if (_poolStack.Count > 0)
                poolable = _poolStack.Pop();
            else
                poolable = Create();

            poolable.transform.SetParent(parent);

            poolable.transform.localPosition = position;
            poolable.transform.rotation = rotation;

            poolable.gameObject.SetActive(true);

            return poolable;
        }
    }
    #endregion

    Dictionary<string, Pool> _pool = new Dictionary<string, Pool>();

    Transform _root;

    protected override void Init()
    {
        base.Init();

        if (_root == null)
        {
            _root = new GameObject { name = "@Pool_Root" }.transform;
        }
    }

    public void CreatePool(GameObject original, int count = 5)
    {
        string key = original.GetInstanceID().ToString();

        if (_pool.ContainsKey(key))
        {
            Debug.LogWarning($"[PoolManager] {original.name} 풀은 이미 존재합니다.");
            return;
        }

        Pool pool = new Pool();
        pool.Init(original, count);
        pool.Root.parent = _root;

        _pool.Add(key, pool);
    }

    public void Push(Poolable poolable)
    {
        string name = poolable.gameObject.name;
        if (_pool.ContainsKey(name) == false)
        {
            Destroy(poolable.gameObject);
            return;
        }

        _pool[name].Push(poolable);
    }

    public T GetFromPool<T>(GameObject original, Transform parent = null, Vector3 position = default, Quaternion rotation = default) where T : Poolable
    {
        string key = original.GetInstanceID().ToString();

        if (!_pool.ContainsKey(key))
            CreatePool(original);

        var poolable = _pool[key].Pop(parent, position, rotation) as T;
        if (poolable == null)
            throw new InvalidCastException($"[PoolManager] {original.name}을(를) {typeof(T)} 타입으로 변환할 수 없습니다. " +
                                           $"해당 프리팹이 {typeof(T)}을(를) 상속하고 있는지 확인하십시오.");

        return poolable;
    }

    public GameObject GetOriginal(string name)
    {
        if (_pool.ContainsKey(name) == false)
            return null;

        return _pool[name].Original;
    }

    public void ClearPool(string poolName)
    {
        if (!_pool.ContainsKey(poolName))
            return;

        Pool pool = _pool[poolName];
        foreach (Transform child in pool.Root)
            Destroy(child.gameObject);

        Destroy(pool.Root.gameObject);
        _pool.Remove(poolName);
    }

    public void ClearAll()
    {
        if (_root == null)
            return;

        foreach (Transform child in _root)
            Destroy(child.gameObject);

        _pool.Clear();
    }
}