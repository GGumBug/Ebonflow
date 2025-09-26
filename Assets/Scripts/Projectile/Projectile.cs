using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectileSystem
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;

        private Vector2 _targetPosition;
        private CancellationTokenSource _cancellationSource;

        private void OnEnable()
        {
            _cancellationSource = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cancellationSource?.Cancel();
            _cancellationSource?.Dispose();
            _cancellationSource = null;
        }

        public void Launch(Vector2 targetPos)
        {
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;

            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            
            MoveAsync(targetPos, _cancellationSource.Token).Forget();
        }

        private async UniTaskVoid MoveAsync(Vector2 targetPos, CancellationToken cancellationToken)
        {
            try
            {
                float duration = Vector2.Distance(transform.position, targetPos) / speed;

                await transform.DOMove(targetPos, duration)
                                .SetEase(Ease.Linear)
                                .ToUniTask(cancellationToken: cancellationToken);

                // 이동 완료후 로직
                // 데미지
            }
            catch
            {
                Debug.Log("Projectile task was cancelled.");
            }
            finally
            {
                gameObject.SetActive(false); // OnDisable()이 호출됨
            }
        }
    }
}