using UnityEngine;

public class DamageCalculator
{
    public int CalculateDamage(UnitStats attacker, UnitStats defender)
    {
        if (attacker == null) return 0;
        // TODO: 방어/저항/관통/치명/난수 등 확장
        int dmg = Mathf.Max(0, attacker.Attack);
        return dmg;
    }
}
