using UnityEngine;

public class ManaGainService
{
    // 공격 시 충전: 기본 20% (최대 10), 메이지/워록/샤먼 40% (최대 20)
    public void OnDealDamage(Unit attacker, int dealtDamage)
    {
        if (attacker == null || attacker.Mana == null || dealtDamage <= 0) return;

        bool caster = IsCaster(attacker);
        float pct = caster ? 0.40f : 0.20f;
        int cap    = caster ? 20    : 10;

        int gain = Mathf.Min(cap, Mathf.CeilToInt(dealtDamage * pct));
        if (gain > 0) attacker.Mana.Add(gain);
        // Debug.Log($"[Mana] Deal +{gain} (dmg={dealtDamage}, caster={caster})");
    }

    // 피격 시 충전: 받은 피해의 10%~20% 랜덤, 최대 50
    public void OnTakeDamage(Unit defender, int takenDamage)
    {
        if (defender == null || defender.Mana == null || takenDamage <= 0) return;

        int min = Mathf.CeilToInt(takenDamage * 0.10f);
        int max = Mathf.CeilToInt(takenDamage * 0.20f);
        // 정수 포함 범위 랜덤
        int rnd = Random.Range(min, max + 1);

        int gain = Mathf.Min(50, rnd);
        if (gain > 0) defender.Mana.Add(gain);
        // Debug.Log($"[Mana] Taken +{gain} (dmg={takenDamage})");
    }

    private bool IsCaster(Unit u)
    {
        var cls = u.Class;
        return cls == UnitClass.Mage || cls == UnitClass.Warlock || cls == UnitClass.Shaman;
    }
}