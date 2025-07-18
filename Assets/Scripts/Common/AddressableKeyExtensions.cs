using System.Collections.Generic;

public enum AddressableKey
{
    AutoBattleUnitPrefab,
    MapGenerationSettings,
    UIMapView,
    NodeTile,
    EdgeView,
    UIAutoBattleShop,
    CardView
}

public static class AddressableKeyExtensions
{
    private static readonly Dictionary<AddressableKey,string> _lookup =
        new Dictionary<AddressableKey,string>
    {
        { AddressableKey.AutoBattleUnitPrefab, "AutoBattle/UnitPrefab" },
        { AddressableKey.MapGenerationSettings, "Config/MapGenSettings" },
        { AddressableKey.UIMapView, "RoguelikeMap/UIMapView" },
        { AddressableKey.UIAutoBattleShop, "Assets/Prefabs/UI/UIAutoBattleShop.prefab" },
        { AddressableKey.CardView, "Assets/Prefabs/UI/CardView.prefab" },
    };

    public static string ToKey(this AddressableKey key)
        => _lookup.TryGetValue(key, out var s) ? s
           : throw new KeyNotFoundException($"[{key}] 키가 등록되지 않았습니다");

    public static AddressableKey ToAddressableKey(this string addressKey)
    {
        foreach (var kv in _lookup.Keys)
        {
            if (kv.ToString() == addressKey)
                return kv;
        }

        throw new KeyNotFoundException($"[{addressKey}]에 해당하는 AddressableKey가 없습니다");
    }
}