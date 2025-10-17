using UnityEngine;

public class UnitCombatAnalyzer
{
    private readonly UnitStats _stats;

    public UnitCombatAnalyzer(UnitStats stats)
    {
        _stats = stats;
    }

    public float GetDPS()
    {
        // AttackDelay가 0인 경우를 방지 (게임 디자인에 따라 처리)
        if (_stats.AttackDelay <= 0)
        {
            Debug.LogWarning("AttackDelay가 0 이하입니다. DPS 계산 불가.");
            return 0f;
        }

        float rawAttack = _stats.Attack;
        float delay = _stats.AttackDelay;

        float simpleDPS = rawAttack / delay;

        // TODO: 여기에 크리티컬, 스킬 계수 등을 반영한 복잡한 DPS 로직 추가

        return simpleDPS;
    }
}
