public class StatModifier
{
    public StatType StatType { get; }
    public ModifierMode Mode { get; }
    public double Value { get; } // Add면 절대값, Mul면 (0.1==+10%)

    public StatModifier(StatType type, ModifierMode mode, double value)
    {
        StatType = type;
        Mode = mode;
        Value = value;
    }
}
