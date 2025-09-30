using Cysharp.Threading.Tasks;
using UnityEngine;
using CombatSystem;
using SkillSystem;
using System;

namespace ProjectileSystem
{
    public class ProjectileManager : Singleton<ProjectileManager>
    {
        private AddressableManager _addressableManager;
        private PoolManager _poolManager;
        private GameObject _projectileOrigin;

        public async UniTask Setup()
        {
            _addressableManager = AddressableManager.Instance;
            _poolManager = PoolManager.Instance;
            _projectileOrigin = await _addressableManager.Load<GameObject>(AddressableKey.BasicProjectile);
        }

        public void LaunchProjectile(IAttacker attacker, IVictim victim, SkillDefinition skillDefinition, ValidationResult validationResult, DamageCalculator damageCalculator, Action<IAttacker, IVictim, DamageCalculator> onApplyDamage)
        {
            Projectile projectile = _poolManager.GetFromPool<Projectile>(_projectileOrigin, null, attacker.Position, Quaternion.identity);
            projectile.SetProjectile(attacker, victim, damageCalculator, onApplyDamage);
            projectile.Launch(victim.Position);
        }
    }
}