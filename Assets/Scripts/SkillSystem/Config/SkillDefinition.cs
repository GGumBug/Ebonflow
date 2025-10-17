using UnityEngine;

namespace SkillSystem
{
    public class SkillDefinition
    {
        public int SkillId { get; private set; }
        public string SkillName { get; private set; }
        public TargetingType Targeting { get; private set; }
        public DeliveryType Delivery { get; private set; }
        public int HitLimit { get; private set; }
        public TargetPriorityType TargetPriorityType { get; private set; }
        public float Coef { get; private set; }
        public AreaShapeType AreaShapeType { get; private set; }
        public float AreaRadius { get; private set; }
        public Vector2 AreaSize { get; private set; }
        public float AreaAngle { get; private set; }

        public SkillDefinition(
        int skillId,
        string skillName,
        TargetingType targeting,
        DeliveryType delivery,
        int hitLimit,
        TargetPriorityType targetPriorityType,
        float coef,
        AreaShapeType areaShapeType,
        float areaRadius,
        Vector2 areaSize,
        float areaAngle)
        {
            SkillId = skillId;
            SkillName = skillName;
            Targeting = targeting;
            Delivery = delivery;
            HitLimit = hitLimit;
            TargetPriorityType = targetPriorityType;
            Coef = coef;

            AreaShapeType = areaShapeType;
            AreaRadius = areaRadius;
            AreaSize = areaSize;
            AreaAngle = areaAngle;
        }
    }
}