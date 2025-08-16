public enum TeamType
{
    Ally,
    Enemy
}

public enum UnitState
{
    Idle,
    Walk,
    Attack,
}

public enum StatType
{
    Hp,
    Attack,
    AttackDelay,
    Range,
    Mana,
}

public enum ModifierMode
{
    Add, 
    Mul,
}

public enum UnitTier
{
    /// <summary>
    /// 필드 전용 적 — 덱에 추가되는 카드 유형이 아닙니다.
    /// </summary>
    Creep = 0,

    /// <summary>
    /// Korean: 혼령 (魂靈)
    /// </summary>
    SoulWisp = 1,

    /// <summary>
    /// Korean: 망혼 (亡魂)
    /// </summary>
    LostSoul = 2,

    /// <summary>
    /// Korean: 사신 (使神)
    /// </summary>
    DeathEnvoy = 3,

    /// <summary>
    /// Korean: 귀장 (鬼將)
    /// </summary>
    GhostGeneral = 4,

    /// <summary>
    /// Korean: 대왕 (大王)
    /// </summary>
    UnderworldKing = 5
}

public enum UnitClass
{
    None,
    Mage,
    Warlock,
    Shaman
}


public enum UnitOrigin
{
    None = 0,
    Human,
    Elf,
    Orc,
    Undead,
    Demon,
    Dragon,
    Spirit,
}
