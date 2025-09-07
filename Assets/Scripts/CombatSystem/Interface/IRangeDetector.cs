using CombatSystem;
using System.Collections.Generic;
using System;

public interface IRangeDetector
{
    /// <summary>현재 감지 반경</summary>
    float DetectionRadius { get; }

    /// <summary>현재 사거리 내 적이 존재하는지 여부</summary>
    bool HasEnemies { get; }

    /// <summary>현재 사거리 내 적 목록(ReadOnly)</summary>
    IReadOnlyCollection<IVictim> EnemiesInRange { get; }

    /// <summary>가장 가까운 적을 반환 (없으면 null)</summary>
    IVictim GetClosestEnemy();

    /// <summary>특정 타겟이 사거리 안에 있는지 확인</summary>
    bool IsTargetInRange(IVictim target);

    /// <summary>사거리 내 적이 모두 사망하거나 나가서 비게 되면 발생</summary>
    event Action OnEnemyListEmpty;

    /// <summary>자신의 팀 정보를 요청할 때 호출 (호출 측에서 제공해야 함)</summary>
    event Func<int> OnRequestTeamId;
}
