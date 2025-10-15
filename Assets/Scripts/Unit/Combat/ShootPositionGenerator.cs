using UnityEngine;

public class ShootPositionGenerator
{
    private const float MaxX = 0.25f;
    private const float MaxY = 0.15f;

    public Vector2 GetShootPositionFromDirection(Vector2 attackerPosition, Vector2 direction)
    {
        float ratioX = Mathf.Abs(direction.x) / MaxX;
        float ratioY = Mathf.Abs(direction.y) / MaxY;

        float scaleFactor = Mathf.Max(ratioX, ratioY);

        if (scaleFactor < Mathf.Epsilon)
        {
            // 방향 벡터가 Vector2.zero인 경우 (예외 처리)
            return attackerPosition;
        }

        Vector2 offset = direction / scaleFactor;

        return attackerPosition + offset;
    }
}
