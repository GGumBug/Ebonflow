using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유닛의 기본 스탯(최대치)과 버프/디버프, HP/마나를 관리합니다.
/// </summary>
public class UnitStats
{
    private static readonly StatType[] AllStatTypes =
        (StatType[])Enum.GetValues(typeof(StatType));

    private readonly StatModifierBucket[] _buckets;
    private UnitCombatAnalyzer _unitCombatAnalyzer;

    public UnitStatData BaseStats { get; private set; }

    public int MaxHP   => _buckets[(int)StatType.Hp].Apply(BaseStats.BaseHp);
    public int Attack  => _buckets[(int)StatType.Attack].Apply(BaseStats.BaseAtk);
    public int Range   => _buckets[(int)StatType.Range].Apply(BaseStats.BaseRange);
    public float AttackDelay => _buckets[(int)StatType.AttackDelay].Apply(BaseStats.BaseAttackDelay);
    public int MaxMana => _buckets[(int)StatType.Mana].Apply(BaseStats.BaseMana);  // ★ 추가

    public int CurrentHP   { get; private set; }
    public int CurrentMana { get; private set; } // ★ 추가
    public float AttackFrameDelay { get; }
    public float ActiveSkillFrameDelay { get; }

    public UnitStats(UnitStatData unitStatData)
    {
        _unitCombatAnalyzer = new UnitCombatAnalyzer(this);
        BaseStats = unitStatData;
        _buckets = new StatModifierBucket[AllStatTypes.Length];
        foreach (var statType in AllStatTypes)
            _buckets[(int)statType] = new StatModifierBucket();

        // 초기화
        CurrentHP   = MaxHP;
        CurrentMana = 0; // 시작은 0 or Max, 게임 규칙에 맞게 설정

        AttackFrameDelay = unitStatData.AttackFrameDelay;
        ActiveSkillFrameDelay = unitStatData.ActiveSkillFrameDelay;

        Debug.Log($"[UnitStats] ★{BaseStats.StarLevel} 생성 → MaxHP={MaxHP}, MaxMana={MaxMana}, CurrentHP={CurrentHP}, CurrentMana={CurrentMana}, Atk={Attack}");
    }

    public float GetDPS() => _unitCombatAnalyzer.GetDPS();

    // --- HP ---
    public void TakeDamage(int damage)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        Debug.Log($"[UnitStats] 데미지 {damage} → CurrentHP={CurrentHP}/{MaxHP}");
    }

    public void Heal(int heal)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + heal);
        Debug.Log($"[UnitStats] 회복 {heal} → CurrentHP={CurrentHP}/{MaxHP}");
    }

    // --- Mana ---
    public bool UseMana(int amount)
    {
        if (CurrentMana < amount) return false;
        CurrentMana -= amount;
        Debug.Log($"[UnitStats] 마나 {amount} 사용 → CurrentMana={CurrentMana}/{MaxMana}");
        return true;
    }

    public void RecoverMana(int amount)
    {
        CurrentMana = Mathf.Min(MaxMana, CurrentMana + amount);
        Debug.Log($"[UnitStats] 마나 {amount} 회복 → CurrentMana={CurrentMana}/{MaxMana}");
    }

    public void FillMana()
    {
        CurrentMana = MaxMana;
        Debug.Log($"[UnitStats] 마나 풀충전 → CurrentMana={CurrentMana}/{MaxMana}");
    }

    public bool IsManaFull()  => CurrentMana >= MaxMana;
    public bool IsManaEmpty() => CurrentMana <= 0;

    // --- Buff/Level ---
    public void AddModifier(StatModifier modifier)
        => _buckets[(int)modifier.StatType].Add(modifier);

    public void RemoveModifier(StatModifier modifier)
        => _buckets[(int)modifier.StatType].Remove(modifier);

    public void ChangeLevel(UnitStatData newBaseStats)
    {
        float hpRatio   = (float)CurrentHP   / MaxHP;
        float manaRatio = (float)CurrentMana / MaxMana;

        BaseStats = newBaseStats;
        Debug.Log($"[UnitStats] 레벨 변경 ★{BaseStats.StarLevel} → MaxHP={MaxHP}, MaxMana={MaxMana}, Atk={Attack}");

        CurrentHP   = Mathf.Clamp(Mathf.RoundToInt(MaxHP * hpRatio), 0, MaxHP);
        CurrentMana = Mathf.Clamp(Mathf.RoundToInt(MaxMana * manaRatio), 0, MaxMana);

        Debug.Log($"[UnitStats] 레벨업 후 HP={CurrentHP}/{MaxHP}, Mana={CurrentMana}/{MaxMana}");
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