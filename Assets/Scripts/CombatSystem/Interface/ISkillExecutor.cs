using Cysharp.Threading.Tasks;
using SkillSystem;
using System;

namespace CombatSystem
{
    public interface ISkillExecutor
    {
        void Execute(IAttacker attacker, SkillDefinition skillDefinition, ValidationResult validationResult, bool isManaGain, Action<IAttacker, Action> startAttackDelegate);
    }
}