using CombatSystem;
using SkillSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AreaSkillExecutor : SkillExecutor
{
    private AreaAttackService _areaAttackService;
    private AreaDebugDrawer _areaDebugDrawer;

    public AreaSkillExecutor()
    {
        _areaAttackService = new AreaAttackService();
        _areaDebugDrawer = new AreaDebugDrawer();
    }

    public override void Execute(IAttacker attacker, SkillDefinition skillDefinition, ValidationResult validationResult, bool isManaGain, Action<IAttacker, Action> startAttackDelegate)
    {
        if (!validationResult.Accepted || validationResult.Targets == null || validationResult.Targets.Count == 0)
        {
            Debug.LogError($"Area skill execution failed validation for {skillDefinition.SkillId}.");
            return;
        }

        Vector2 centerPosition = validationResult.Targets[0].Position;

        Vector2 direction = (centerPosition - (Vector2)attacker.Position).normalized;
        attacker.Model.SetUnitDirection(direction);

        startAttackDelegate.Invoke(attacker, () =>
        {
            _areaDebugDrawer.DrawArea(
                centerPosition,
                skillDefinition.AreaShapeType,
                skillDefinition.AreaRadius,
                skillDefinition.AreaSize,
                skillDefinition.AreaAngle,
                direction
            );

            List<Unit> enemiesInArea = _areaAttackService.GetTargetsInArea(
                centerPosition,
                skillDefinition.AreaShapeType,
                skillDefinition.AreaRadius,
                skillDefinition.AreaSize,
                skillDefinition.AreaAngle,
                (TeamType)attacker.TeamId, // Used to determine the target layer (enemies)
                direction           // Used as the forward vector for Cone attacks
            );

            if (enemiesInArea.Count == 0)
            {
                Debug.Log($"Area attack missed all targets at {centerPosition}.");
                return;
            }

            foreach (var enemy in enemiesInArea)
            {
                ApplyDamage(attacker, enemy, combatManager.Calculator, isManaGain);
            }
        });
    }
}