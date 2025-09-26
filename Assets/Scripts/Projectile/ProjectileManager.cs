using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

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

        public void LaunchProjectile(Vector2 startPos, Vector2 targetPos, int damage)
        {
            Projectile projectile = _poolManager.GetFromPool<Projectile>(_projectileOrigin, null, startPos, Quaternion.identity);
            projectile.Launch(targetPos);
        }
    }
}