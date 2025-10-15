using System;

[Serializable]
public class UnitStatData
{
    public int UnitId { get; }
    public int StarLevel { get; }
    public int BaseHp { get; }
    public int BaseAtk { get; }
    public int BaseRange { get; }
    public float BaseAttackDelay { get; }
    public int BaseMana { get; }
    public float AttackFrameDelay { get; }
    public float ActiveSkillFrameDelay { get; }

    public UnitStatData(DB_UnitStats e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        UnitId = e.f_UnitId;
        StarLevel = e.f_StarLevel;
        BaseHp = e.f_Health;
        BaseAtk = e.f_Attack;
        BaseRange = e.f_Range;
        BaseAttackDelay = e.f_AttackDelay;
        BaseMana = e.f_Mana;
        AttackFrameDelay = e.f_AttackFrameDelay;
        ActiveSkillFrameDelay = e.f_ActiveSkillFrameDelay;
    }
}