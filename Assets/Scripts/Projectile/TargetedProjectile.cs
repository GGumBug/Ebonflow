using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectileSystem
{
    public class TargetedProjectile : Projectile
    {
        protected override void SetDestination()
        {
            var offsetDest = new Vector2(_victim.Position.x, _victim.Position.y + TARGET_Y_OFFSET);
            _destination = offsetDest;
        }

        protected override void CheckComplete()
        {
            if (Vector2.Distance(transform.position, _destination) < 0.1f)
            {
                CompleteActionAndReturnToPool();
            }
        }
    }
}