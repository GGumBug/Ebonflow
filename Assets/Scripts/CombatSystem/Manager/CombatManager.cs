using SkillSystem;
using UnityEngine;

namespace CombatSystem
{
    public class CombatManager : Singleton<CombatManager>
    {
        private DamageCalculator _damageCalculator;
        private ManaGainService _manaGainService;
        private SkillRepository _skillRepository;

        public void Setup()
        {
            _skillRepository = new SkillRepository();
            _damageCalculator = new DamageCalculator();
            _manaGainService = new ManaGainService();
        }

        public bool Trigger(int skillId, IAttacker attacker, IRangeDetector detector)
        {
            SkillDefinition currentSkill = null;

            bool getSkillDefinitionResult = _skillRepository.TryGet(skillId, out currentSkill);

            if (!getSkillDefinitionResult || currentSkill == null)
            {
                Debug.LogError("스킬 데이터 가져오기 실패.");
                return false;
            }

            return true;
        }
    }
}