using System.Collections.Generic;

public enum AddressableKey
{
    AutoBattleUnitPrefab,
    MapGenerationSettings,
    UIMapView,
    NodeTile,
    EdgeView
}

public static class AddressableKeyExtensions
{
    private static readonly Dictionary<AddressableKey,string> _lookup =
        new Dictionary<AddressableKey,string>
    {
        { AddressableKey.AutoBattleUnitPrefab, "AutoBattle/UnitPrefab" },
        { AddressableKey.MapGenerationSettings, "Config/MapGenSettings" },
        { AddressableKey.UIMapView, "RoguelikeMap/UIMapView" },
    };

    public static string ToKey(this AddressableKey key)
        => _lookup.TryGetValue(key, out var s) ? s
           : throw new KeyNotFoundException($"[{key}] 키가 등록되지 않았습니다");
}