using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유닛의 기본 스탯(최대치)과 버프/디버프, 그리고 현재 체력을 관리합니다.
/// </summary>
public class UnitStats
{
    // StatType 열거형 값을 한 번만 구해서 재사용
    private static readonly StatType[] AllStatTypes =
        (StatType[])Enum.GetValues(typeof(StatType));

    private readonly StatModifierBucket[] _buckets;

    public UnitStatData BaseStats { get; private set; }
    public int MaxHP => _buckets[(int)StatType.Hp].Apply(BaseStats.BaseHp);
    public int Attack => _buckets[(int)StatType.Attack].Apply(BaseStats.BaseAtk);
    public int Range => _buckets[(int)StatType.Range].Apply(BaseStats.BaseRange);
    public float AttackDelay => _buckets[(int)StatType.AttackDelay].Apply(BaseStats.BaseAttackDelay);

    /// <summary>현재 남아 있는 HP. 데미지를 받거나 회복하면 이 값을 변경합니다.</summary>
    public int CurrentHP { get; private set; }

    public UnitStats(UnitStatData unitStatData)
    {
        BaseStats = unitStatData;
        _buckets = new StatModifierBucket[AllStatTypes.Length];
        foreach (var statType in AllStatTypes)
            _buckets[(int)statType] = new StatModifierBucket();

        // 생성 시 현재 HP를 최대 HP로 초기화
        CurrentHP = MaxHP;

        Debug.Log($"[UnitStats] ★{BaseStats.StarLevel} 생성 → MaxHP={MaxHP}, CurrentHP={CurrentHP}, MaxAtk={Attack}");
    }

    /// <summary>
    /// 유닛이 데미지를 받습니다.
    /// </summary>
    /// <param name="damage">받을 데미지</param>
    public void TakeDamage(int damage)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        Debug.Log($"[UnitStats] 데미지 {damage} 적용 → CurrentHP={CurrentHP}/{MaxHP}");
        if (CurrentHP == 0)
        {
            // 예: OnDied 이벤트 호출
        }
    }

    /// <summary>
    /// 유닛이 회복합니다.
    /// </summary>
    /// <param name="heal">회복량</param>
    public void Heal(int heal)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + heal);
        Debug.Log($"[UnitStats] 회복 {heal} 적용 → CurrentHP={CurrentHP}/{MaxHP}");
    }

    public void AddModifier(StatModifier modifier)
        => _buckets[(int)modifier.StatType].Add(modifier);

    public void RemoveModifier(StatModifier modifier)
        => _buckets[(int)modifier.StatType].Remove(modifier);

    /// <summary>
    /// 별 레벨이 변경될 때, 새로운 baseStats를 받아와 MaxHP/MaxAtk를 갱신합니다.
    /// CurrentHP는 그대로 유지하거나, 최대치 비율에 맞춰 조정할 수 있습니다.
    /// </summary>
    public void ChangeLevel(UnitStatData newBaseStats)
    {
        float hpRatio = (float)CurrentHP / MaxHP;

        BaseStats = newBaseStats;
        Debug.Log($"[UnitStats] 레벨 변경 ★{BaseStats.StarLevel} → MaxHP={MaxHP}, MaxAtk={Attack}");

        // 기존 남은 체력 비율 유지
        CurrentHP = Mathf.Clamp(Mathf.RoundToInt(MaxHP * hpRatio), 0, MaxHP);
        Debug.Log($"[UnitStats] 레벨업 후 CurrentHP 비율 유지 → CurrentHP={CurrentHP}/{MaxHP}");
    }

    // --- 내부용 modifier 버킷 ---
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