using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class SceneLoadCallbacks
{
    public Func<UniTask> LoadAssets { get; set; }
    public Func<UniTask> InitializeData { get; set; }
    public Func<UniTask> SetupScene { get; set; }
    public Func<UniTask> FinalizeLoading { get; set; }

    public SceneLoadCallbacks(
        Func<UniTask> loadAssets,
        Func<UniTask> initializeData,
        Func<UniTask> setupScene,
        Func<UniTask> finalizeLoading)
    {
        LoadAssets = loadAssets ?? throw new ArgumentNullException(nameof(loadAssets));
        InitializeData = initializeData ?? throw new ArgumentNullException(nameof(initializeData));
        SetupScene = setupScene ?? throw new ArgumentNullException(nameof(setupScene));
        FinalizeLoading = finalizeLoading ?? throw new ArgumentNullException(nameof(finalizeLoading));
    }
}
