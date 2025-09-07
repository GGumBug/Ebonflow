using CombatSystem;
using UnityEngine;

namespace SkillSystem
{
    [CreateAssetMenu(menuName = "Combat/SkillDefinition")]
    public class SkillDefinition : ScriptableObject
    {
        public string SkillId = "BasicAttack";
        public TargetingType Targeting = TargetingType.Targeted;
        public DeliveryType Delivery = DeliveryType.Instant;

        [Header("Range & Limits")]
        public float Range = 1.8f;       // 근접 사거리
        public int HitLimit = 1;         // 기본공격은 1명

        [Header("Validation")]
        public CastValidationPolicy Validation = CastValidationPolicy.RequireEnemyInRange;

        [Header("Damage Coef")]
        public float Coef = 1.0f;        // 공격력 계수
    }
}