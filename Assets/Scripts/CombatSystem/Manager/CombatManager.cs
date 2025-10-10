using SkillSystem;
using UnityEngine;

namespace CombatSystem
{
    public class CombatManager : Singleton<CombatManager>
    {
        private DamageCalculator _damageCalculator;
        private ManaGainService _manaGainService;
        private SkillRepository _skillRepository;
        private CastValidatorFactory _castValidatorFactory;
        private SkillExecutorFactory _skillExecutorFactory;

        public void Setup()
        {
            _skillRepository = new SkillRepository();
            _damageCalculator = new DamageCalculator();
            _manaGainService = new ManaGainService();
            _castValidatorFactory = new CastValidatorFactory();
            _skillExecutorFactory = new SkillExecutorFactory();
        }

        public bool Trigger(IAttacker attacker, IRangeDetector detector)
        {
            SkillDefinition currentSkill = null;

            bool getSkillDefinitionResult = _skillRepository.TryGet(attacker.AttackSkillID, out currentSkill);

            if (!getSkillDefinitionResult || currentSkill == null)
            {
                Debug.LogError("스킬 데이터 가져오기 실패.");
            }

            // 타겟 적 혹은 방향 값 위치값 담은 결과
            ValidationResult validationResult = _castValidatorFactory.Get(currentSkill.Targeting).Validate(currentSkill, attacker, detector);

            if (validationResult.Targets == null || validationResult.Targets.Count <= 0)
            {
                return false;
            }

            // 타겟 논타겟 에리어에 따라 또 전략이 나눠져야 될 거 같아.
            _skillExecutorFactory.GetExecutor(currentSkill.Targeting).Execute(
                attacker,
                currentSkill,
                validationResult,
                _damageCalculator
            );

            return true;
        }
    }
}