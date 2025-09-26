using System.Collections.Generic;
using CombatSystem;

public class SkillExecutorFactory
{
    private readonly Dictionary<TargetingType, ISkillExecutor> _executorsByTargetingType;

    public SkillExecutorFactory()
    {
        _executorsByTargetingType = new Dictionary<TargetingType, ISkillExecutor>{
            { TargetingType.Targeted, new TargetedSkillExecutor() },
        };
    }

    public ISkillExecutor GetExecutor(TargetingType targetingType)
    {
        if (_executorsByTargetingType.TryGetValue(targetingType, out var executor))
        {
            return executor;
        }

        throw new System.Exception($"Unsupported TargetingType: {targetingType}");
    }
}
