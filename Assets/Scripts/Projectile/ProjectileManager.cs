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
        private GameObject _basicProjectileOrigin;
        private GameObject _triggerProjectileOrigin;

        public async UniTask Setup()
        {
            _addressableManager = AddressableManager.Instance;
            _poolManager = PoolManager.Instance;
            _basicProjectileOrigin = await _addressableManager.Load<GameObject>(AddressableKey.TargetedProjectile);
            _triggerProjectileOrigin = await _addressableManager.Load<GameObject>(AddressableKey.SkillshotProjectile);
        }

        public void LaunchProjectile(IAttacker attacker, IVictim victim, SkillDefinition skillDefinition, ValidationResult validationResult, DamageCalculator damageCalculator, bool isManaGain, Action<IAttacker, IVictim, DamageCalculator, bool> onApplyDamage, Vector2 destination = default)
        {
            Projectile projectile = null;
            Vector2 direction = Vector2.zero;
            Vector2 shootPosition = Vector2.zero;

            switch (skillDefinition.Targeting)
            {
                case TargetingType.Targeted:
                    direction = ((Vector2)victim.Position - (Vector2)attacker.Position).normalized;
                    shootPosition = attacker.Model.GetShootPositionFromDirection(direction);
                    projectile = _poolManager.GetFromPool<TargetedProjectile>(_basicProjectileOrigin, null, shootPosition, Quaternion.identity);
                    break;
                case TargetingType.Skillshot:
                    direction = (destination - (Vector2)attacker.Position).normalized;
                    shootPosition = attacker.Model.GetShootPositionFromDirection(direction);
                    projectile = _poolManager.GetFromPool<SkillshotProjectile>(_triggerProjectileOrigin, null, shootPosition, Quaternion.identity);
                    break;
            }

            projectile.SetData(attacker, victim, destination, damageCalculator, isManaGain, onApplyDamage);
            projectile.Launch();
        }
    }
}