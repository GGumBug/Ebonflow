using UnityEngine;
using System.Collections.Generic;

namespace SkillSystem
{
    public class SkillRepository
    {
        private readonly Dictionary<int, SkillDefinition> _skillDefinitionsbyId;

        public bool TryGet(int skillId, out SkillDefinition def)
            => _skillDefinitionsbyId.TryGetValue(skillId, out def);

        public IReadOnlyCollection<SkillDefinition> GetAll()
            => _skillDefinitionsbyId.Values;

        public SkillRepository()
        {
            _skillDefinitionsbyId = new Dictionary<int, SkillDefinition>();

            var allSkillEntities = DB_SkillDefinitions.FindEntities(e => true);

            if (allSkillEntities == null || allSkillEntities.Count <= 0)
            {
                Debug.LogError("SkillDefinitions 테이블 로드 실패.");
            }

            foreach (var e in allSkillEntities)
            {
                SkillDefinition newSkillDef = new SkillDefinition(
                    skillId: e.f_SkillID,
                    skillName: e.f_name,
                    targeting: e.f_TargetingType,
                    delivery: e.f_DeliveryType,
                    hitLimit: e.f_HitLimit,
                    targetPriorityType: e.f_TargetPriorityType,
                    coef: e.f_Coef,
                    areaShapeType: e.f_AreaShapeType,
                    areaRadius: e.f_AreaRadius,
                    areaSize: e.f_AreaSize,
                    areaAngle: e.f_AreaAngle
                    );

                if (!_skillDefinitionsbyId.ContainsKey(e.f_SkillID))
                {
                    _skillDefinitionsbyId.Add(e.f_SkillID, newSkillDef);
                }
                else
                {
                    Debug.LogWarning($"중복된 SkillId 발견: {e.f_SkillID}");
                }
            }
        }
    }
}