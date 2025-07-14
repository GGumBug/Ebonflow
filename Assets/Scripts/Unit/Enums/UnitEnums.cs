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
}

public enum ModifierMode
{
    Add, 
    Mul,
}

public enum UnitTier
{
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
