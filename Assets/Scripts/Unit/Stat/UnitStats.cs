using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 유닛의 기본 스탯과 버프/디버프를 관리합니다.
/// </summary>
public class UnitStats
{
    // StatType 열거형 값을 한 번만 구해서 재사용
    private static readonly StatType[] AllStatTypes =
        (StatType[])Enum.GetValues(typeof(StatType));

    private readonly StatModifierBucket[] _buckets;
    private UnitStatData _baseStats;

    public UnitStats(UnitStatData unitStatData)
    {
        _baseStats = unitStatData;
        _buckets = new StatModifierBucket[AllStatTypes.Length];
        foreach (var statType in AllStatTypes)
            _buckets[(int)statType] = new StatModifierBucket();

        Debug.Log($"Create to ★{_baseStats.StarLevel}. Current HP={CurrentHP}, ATK={CurrentAttack}");
    }

    /// <summary>현재 HP (버프/디버프 적용 후 값)</summary>
    public int CurrentHP => _buckets[(int)StatType.Hp].Apply(_baseStats.BaseHp);

    /// <summary>현재 공격력 (버프/디버프 적용 후 값)</summary>
    public float CurrentAttack => _buckets[(int)StatType.Attack].Apply(_baseStats.BaseAtk);

    public void AddModifier(StatModifier modifier)
        => _buckets[(int)modifier.StatType].Add(modifier);

    public void RemoveModifier(StatModifier modifier)
        => _buckets[(int)modifier.StatType].Remove(modifier);

    /// <summary>
    /// 유닛의 ★레벨을 변경하고, 그에 맞는 기본 스탯을 다시 로드합니다.
    /// 기존에 추가된 모든 ModifierBucket(버프/디버프)들은 유지됩니다.
    /// </summary>
    /// <param name="newStarLevel">새로 적용할 별레벨</param>
    public void ChangeLevel(UnitStatData unitStatData)
    {
        // _baseStats 를 새 레벨에 맞춰 다시 불러옵니다.
        _baseStats = unitStatData;

        // _buckets 는 재생성하지 않으므로 기존 버프/디버프 유지
        Debug.Log($"Changed to ★{_baseStats.StarLevel}. Current HP={CurrentHP}, ATK={CurrentAttack}");
    }

    /// <summary>
    /// 각 StatType 별로 더하기/곱하기 모디파이어를 관리합니다.
    /// </summary>
    private sealed class StatModifierBucket
    {
        private double _addSum;
        private double _mulProd = 1;
        private readonly List<StatModifier> _modifiers = new();

        public void Add(StatModifier m)
        {
            if (m.Mode == ModifierMode.Add) _addSum += m.Value;
            else _mulProd *= 1 + m.Value;
            _modifiers.Add(m);
        }

        public void Remove(StatModifier m)
        {
            if (!_modifiers.Remove(m)) return;
            if (m.Mode == ModifierMode.Add) _addSum -= m.Value;
            else _mulProd /= 1 + m.Value;
        }

        public int Apply(int baseValue)
            => (int)((baseValue + _addSum) * _mulProd);

        public float Apply(float baseValue)
            => (float)((baseValue + _addSum) * _mulProd);
    }
}