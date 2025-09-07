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
        public CastValidationPolicy Validation { get; private set; }
        public float Coef { get; private set; }

        public SkillDefinition(
        int skillId,
        string skillName,
        TargetingType targeting,
        DeliveryType delivery,
        int hitLimit,
        CastValidationPolicy validation,
        float coef)
        {
            SkillId = skillId;
            SkillName = skillName;
            Targeting = targeting;
            Delivery = delivery;
            HitLimit = hitLimit;
            Validation = validation;
            Coef = coef;
        }
    }
}