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

        public SkillDefinition(
        int skillId,
        string skillName,
        TargetingType targeting,
        DeliveryType delivery,
        int hitLimit,
        TargetPriorityType targetPriorityType,
        float coef)
        {
            SkillId = skillId;
            SkillName = skillName;
            Targeting = targeting;
            Delivery = delivery;
            HitLimit = hitLimit;
            TargetPriorityType = targetPriorityType;
            Coef = coef;
        }
    }
}