using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class AddressableManager : Singleton<AddressableManager>
{
    private Dictionary<string, AsyncOperationHandle> _loadedAssets = new();

    public async UniTask<T> Load<T>(string addressKey) where T : class
    {
        if (_loadedAssets.TryGetValue(addressKey, out var handle))
        {
            if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                return handle.Result as T;
            else
                _loadedAssets.Remove(addressKey);
        }

        handle = Addressables.LoadAssetAsync<T>(addressKey);
        _loadedAssets[addressKey] = handle;

        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load addressable asset: {addressKey}");
            _loadedAssets.Remove(addressKey);
            return default;
        }

        var result = handle.Result as T;
        if (result == null)
        {
            Debug.LogError($"Type mismatch: Addressable asset {addressKey} is not of type {typeof(T)}");
            _loadedAssets.Remove(addressKey);
            return default;
        }

        return result;
    }

    public async UniTask<GameObject> InstantiateAsync(string addressKey, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        var prefab = await Load<GameObject>(addressKey);
        var go = Instantiate(prefab, position, rotation, parent);
        return go;
    }

    public async UniTask<T> InstantiateAsync<T>(string addressKey, Vector3 position = default, Quaternion rotation = default, Transform parent = null) where T : Component
    {
        var prefab = await Load<GameObject>(addressKey);
        var go = Instantiate(prefab, position, rotation, parent);
        var comp = go.GetComponent<T>();
        if (comp == null)
            throw new InvalidOperationException(
                $"[{addressKey}] 프리팹에 {typeof(T).Name} 컴포넌트가 없습니다."
            );
        return comp;
    }

    public void Unload(string addressKey)
    {
        if (_loadedAssets.TryGetValue(addressKey, out var handle) && handle.IsValid())
            Addressables.Release(handle);

        _loadedAssets.Remove(addressKey);
    }

    public void UnloadAll()
    {
        foreach (var pair in _loadedAssets)
        {
            var handle = pair.Value;
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        _loadedAssets.Clear();
    }

    private void OnDestroy()
    {
        UnloadAll();
    }
}

