using UnityEngine;

namespace CombatSystem
{
    public readonly struct ValidationResult
    {
        public bool Accepted { get; }
        public string Reason { get; }

        /// <summary>Targeted 스킬의 경우 타겟 유닛</summary>
        public IVictim Target { get; }

        /// <summary>에리어 스킬의 경우 조준 위치(월드 좌표)</summary>
        public Vector3 AimPoint { get; }

        /// <summary>스킬샷의 경우 조준 방향(단위 벡터)</summary>
        public Vector3 AimDirection { get; }

        public ValidationResult(
            bool accepted,
            string reason = null,
            IVictim target = null,
            Vector3 aimPoint = default,
            Vector3 aimDirection = default)
        {
            Accepted = accepted;
            Reason = reason;
            Target = target;
            AimPoint = aimPoint;
            AimDirection = aimDirection;
        }

        public static ValidationResult Ok(
            IVictim target = null,
            Vector3 aimPoint = default,
            Vector3 aimDirection = default)
            => new ValidationResult(true, null, target, aimPoint, aimDirection);

        public static ValidationResult Fail(string reason)
            => new ValidationResult(false, reason);
    }
}