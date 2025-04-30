using System;

struct EdgeKey : IEquatable<EdgeKey>
{
    private const int HashMultiplier = 31;
    public readonly int A;
    public readonly int B;

    public EdgeKey(int a, int b)
    {
        // 작은 쪽을 A에, 큰 쪽을 B에
        if (a < b) { A = a; B = b; }
        else       { A = b; B = a; }
    }

    public bool Equals(EdgeKey other) => A == other.A && B == other.B;
    public override bool Equals(object o) => o is EdgeKey ek && Equals(ek);
    public override int GetHashCode() => (A * HashMultiplier) ^ B;
}
