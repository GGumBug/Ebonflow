using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using DG.Tweening;

namespace ProjectileSystem
{
    public class SkillshotProjectile : Projectile
    {
        [SerializeField] private LayerMask targetLayerMask;

        void OnTriggerEnter2D(Collider2D collision)
        {
            if ((targetLayerMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                Unit triggeredUnit = null;
                if (collision.gameObject.TryGetComponent<Unit>(out triggeredUnit))
                {
                    if (_attacker.TeamId != triggeredUnit.TeamId)
                    {
                        CompleteActionAndReturnToPool();
                    }
                }
            }
        }

        protected override void SetDestination()
        {
            var offsetDest = new Vector2(_destination.x, _destination.y + TARGET_Y_OFFSET);
            _destination = offsetDest;
        }

        protected override void CheckComplete()
        {
            
        }
    }
}