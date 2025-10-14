using System;

[Serializable]
public class UnitData
{
    public int UnitId { get; }
    public string UnitCode { get; }
    public int NameKey { get; }
    public UnitTier UnitTier { get; }
    public UnitClass Class { get; }
    public UnitOrigin Origin { get; }
    public int AttackSkillID { get; }
    public int ActiveSkillID { get; }
    public AddressableKey UnitAnimatorKey { get; }

    public UnitData(DB_Units e)
    {
        if (e == null)
            throw new ArgumentNullException(nameof(e));

        UnitId = e.f_UnitId;
        UnitCode = e.f_UnitCode;
        NameKey = e.f_NameKey;
        UnitTier = e.f_UnitTier;
        Class = e.f_Class;
        Origin = e.f_Origin;
        AttackSkillID = e.f_AttackSkillId;
        ActiveSkillID = e.f_ActiveSkillId;
        UnitAnimatorKey = e.f_ModelKey;
    }
}
