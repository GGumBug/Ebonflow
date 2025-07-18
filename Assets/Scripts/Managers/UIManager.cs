using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI 생성, 검색, 열기/닫기 등을 관리하는 싱글톤 매니저 클래스
/// </summary>
public class UIManager : Singleton<UIManager>, IDonDestroy
{
    private readonly Dictionary<Type, GameObject> _uiRegistry = new Dictionary<Type, GameObject>();
    private Transform _uiRoot;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private GameObject _eventSystemObject;

    /// <summary>
    /// UI 루트 Transform (씬 전환 시 파괴되지 않음)
    /// </summary>
    public Transform UIRoot
    {
        get
        {
            if (_uiRoot == null)
            {
                var rootObj = GameObject.Find("UIRoot") ?? new GameObject("UIRoot");
                _uiRoot = rootObj.transform;
            }
            return _uiRoot;
        }
    }

    /// <summary>
    /// EventSystem이 존재하지 않으면 생성합니다.
    /// </summary>
    private void EnsureEventSystem()
    {
        if (_eventSystemObject != null) return;
        if (FindFirstObjectByType<EventSystem>() != null) return;

        _eventSystemObject = new GameObject("@EventSystem");
        _eventSystemObject.AddComponent<EventSystem>();
        _eventSystemObject.AddComponent<StandaloneInputModule>();
        DontDestroyOnLoad(_eventSystemObject);
    }

    /// <summary>
    /// Addressable 키를 기반으로 UI를 생성하고 반환합니다.
    /// </summary>
    public async UniTask<T> CreateUIAsync<T>() where T : UIBase
    {
        EnsureEventSystem();

        // T 타입 이름으로 AddressableKey를 가져옵니다.
        var keyName = typeof(T).Name;
        if (!Enum.TryParse<AddressableKey>(keyName, ignoreCase: true, out var key))
            throw new KeyNotFoundException($"[{keyName}] AddressableKey에 등록되지 않았습니다.");

        // AddressableManager의 Load 메서드를 사용해 프리팹을 로드합니다.
        var go = await AddressableManager.Instance.InstantiateAsync(key, default, default, UIRoot);
        if (go == null)
            throw new InvalidOperationException($"[{key}] 에셋 로드 실패: {key.ToKey()}");

        // 캔버스가 있으면 메인 카메라 설정
        if (go.TryGetComponent<Canvas>(out var canvas))
            canvas.worldCamera = Camera.main;

        if (!go.TryGetComponent<T>(out var component))
            throw new InvalidOperationException($"{keyName} 프리팹에 {typeof(T).Name} 컴포넌트가 없습니다.");

        _uiRegistry[typeof(T)] = go;
        return component;
    }

    /// <summary>
    /// 기존 UI를 반환하거나 없으면 생성합니다.
    /// </summary>
    public async UniTask<T> GetUIAsync<T>() where T : UIBase
    {
        await _lock.WaitAsync();
        try
        {
            if (_uiRegistry.TryGetValue(typeof(T), out var go) && go != null)
                return go.GetComponent<T>();

            return await CreateUIAsync<T>();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// UI를 활성화하거나 없으면 생성 후 활성화합니다.
    /// </summary>
    public async UniTask<T> OpenUIAsync<T>() where T : UIBase
    {
        var ui = await GetUIAsync<T>();
        ui.gameObject.SetActive(true);
        return ui;
    }

    /// <summary>
    /// UI를 비활성화합니다.
    /// </summary>
    public void CloseUI<T>() where T : UIBase
    {
        if (_uiRegistry.TryGetValue(typeof(T), out var go) && go != null)
            go.SetActive(false);
    }

    /// <summary>
    /// 모든 UI 인스턴스를 파괴하고 레지스트리를 초기화합니다.
    /// </summary>
    public void ClearAllUI()
    {
        foreach (var go in _uiRegistry.Values)
            Destroy(go);

        _uiRegistry.Clear();

        if (_uiRoot != null)
        {
            foreach (Transform child in _uiRoot)
                Destroy(child.gameObject);
        }
    }

    private void OnDestroy()
    {
        ClearAllUI();
        if (_eventSystemObject != null)
            Destroy(_eventSystemObject);
    }
}