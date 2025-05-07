using System.Collections.Generic;

public enum AddressableKey
{
    AutoBattleUnitPrefab,
    MapGenerationSettings,
    // …추가
}

public static class AddressableKeyExtensions
{
    private static readonly Dictionary<AddressableKey,string> _lookup =
        new Dictionary<AddressableKey,string>
    {
        { AddressableKey.AutoBattleUnitPrefab, "AutoBattle/UnitPrefab" },
        { AddressableKey.MapGenerationSettings, "Config/MapGenSettings" },
        // …실제 Addressables의 키
    };

    public static string ToKey(this AddressableKey key)
        => _lookup.TryGetValue(key, out var s) ? s
           : throw new KeyNotFoundException($"[{key}] 키가 등록되지 않았습니다");
}