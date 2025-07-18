using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class AddressableManager : Singleton<AddressableManager>
{
    private readonly Dictionary<AddressableKey, AsyncOperationHandle> _loadedAssets =
        new Dictionary<AddressableKey, AsyncOperationHandle>();

    public UniTask<T> Load<T>(AddressableKey key) where T : class
        => Load<T>(key.ToKey(), key);

    public UniTask<T> InstantiateAsync<T>(AddressableKey key, Vector3 position = default, Quaternion rotation = default, Transform parent = null) where T : Component
        => InstantiateAsync<T>(key.ToKey(), key, position, rotation, parent);

    public async UniTask<GameObject> InstantiateAsync(AddressableKey key, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        string address = key.ToKey();
        return await InstantiateAsync(address, key, position, rotation, parent);
    }

    private async UniTask<T> Load<T>(string address, AddressableKey key) where T : class
    {
        if (_loadedAssets.TryGetValue(key, out var handle))
        {
            if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                return handle.Result as T;
            _loadedAssets.Remove(key);
        }

        handle = Addressables.LoadAssetAsync<T>(address);
        _loadedAssets[key] = handle;

        await handle.Task;
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[{key}] 에셋 로드 실패: {address}");
            _loadedAssets.Remove(key);
            return default;
        }

        if (handle.Result is T result)
            return result;

        Debug.LogError($"[{key}] 형식 불일치: {typeof(T)}");
        _loadedAssets.Remove(key);
        return default;
    }

    /// <summary>
    /// AddressableKey 기반으로 GameObject를 인스턴스화합니다.
    /// </summary>
    private async UniTask<GameObject> InstantiateAsync(string address, AddressableKey key,
        Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        var prefab = await Load<GameObject>(key);
        return Instantiate(prefab, position, rotation, parent);
    }

    /// <summary>
    /// AddressableKey 기반으로 T 컴포넌트를 인스턴스화합니다.
    /// </summary>
    private async UniTask<T> InstantiateAsync<T>(string address, AddressableKey key,
        Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        where T : Component
    {
        var go = await InstantiateAsync(address, key, position, rotation, parent);
        if (go.TryGetComponent<T>(out var comp))
            return comp;

        throw new InvalidOperationException(
            $"[{key}] 프리팹에 {typeof(T).Name} 컴포넌트가 없습니다.");
    }

    /// <summary>
    /// AddressableKey 기반으로 에셋을 언로드합니다.
    /// </summary>
    public void Unload(AddressableKey key)
    {
        if (_loadedAssets.TryGetValue(key, out var handle) && handle.IsValid())
            Addressables.Release(handle);
        _loadedAssets.Remove(key);
    }

    /// <summary>
    /// 모든 로드된 에셋 언로드
    /// </summary>
    public void UnloadAll()
    {
        foreach (var kv in _loadedAssets)
        {
            if (kv.Value.IsValid())
                Addressables.Release(kv.Value);
        }
        _loadedAssets.Clear();
    }

    private void OnDestroy() => UnloadAll();
}