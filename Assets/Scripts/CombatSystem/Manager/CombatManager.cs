using System;
using UnityEngine;

namespace CombatSystem
{
    public class CombatManager : Singleton<CombatManager>
    {
        private DamageCalculator _damageCalculator;
        private ManaGainService _manaGainService;

        public void Setup()
        {
            _damageCalculator = new DamageCalculator();
            _manaGainService = new ManaGainService();
        }

        public bool Attack(IAttacker attacker, IVictim defender)
        {
            if (attacker == null) throw new ArgumentNullException(nameof(attacker));
            if (defender == null || defender.IsDead)
                throw new Exception("defender was null or dead.");

            // 1) 최종 데미지 계산
            var atkStats = attacker.Stat;
            var defStats = defender.Stat;
            int rawDamage = Mathf.Max(0, _damageCalculator.CalculateDamage(atkStats, defStats));

            // 2) HP 적용(실제 적용량 확보)
            int appliedDamage;
            bool killed = defender.Health.ApplyDamageAndGetApplied(rawDamage, out appliedDamage);

            // 3) 마나 충전(실제 적용량 기준)
            if (appliedDamage > 0)
            {
                _manaGainService?.OnDealDamage(attacker, appliedDamage);
                // _manaGainService?.OnTakeDamage(defender, appliedDamage); 피해 받을 때 마나회복
            }

            return killed;
        }
    }
}