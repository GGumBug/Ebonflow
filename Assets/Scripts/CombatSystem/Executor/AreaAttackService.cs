using System;
using System.Collections.Generic;
using UnityEngine;

public class AreaAttackService
{
    private readonly int UNIT_MASK = LayerMask.GetMask("Unit");

    public List<Unit> GetTargetsInArea(
        Vector2 center,
        AreaShapeType shape,
        float radius,
        Vector2 size,
        float angle,
        TeamType attackerTeam,
        Vector2 forwardDir
    )
    {
        Collider2D[] potentialHits;

        potentialHits = shape switch
        {
            AreaShapeType.Circle or AreaShapeType.Cone => Physics2D.OverlapCircleAll(center, radius, UNIT_MASK),
            AreaShapeType.Box => Physics2D.OverlapBoxAll(center, size, angle, UNIT_MASK),
            _ => Array.Empty<Collider2D>()
        };

        List<Unit> finalTargets = new List<Unit>();
        foreach (var hit in potentialHits)
        {
            if (hit.TryGetComponent(out Unit enemy) && attackerTeam != enemy.GetTeam() && !enemy.IsDead)
            {
                bool isValid = true;

                if (shape == AreaShapeType.Cone)
                {
                    Vector2 directionToTarget = ((Vector2)enemy.Position - center).normalized;
                    float angleToTarget = Vector2.Angle(forwardDir, directionToTarget);

                    if (angleToTarget > angle / 2f)
                    {
                        isValid = false;
                    }
                }

                if (isValid)
                {
                    finalTargets.Add(enemy);
                }
            }
        }
        return finalTargets;
    }
}
