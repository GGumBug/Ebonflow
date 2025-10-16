using UnityEngine;

public class ShootPositionGenerator
{
    private static readonly Vector2[] LOCAL_OFFSETS = new Vector2[]
    {
        // 0: E (0.675, 0.875)
        new Vector2(0.675f, 0.875f),
        // 1: NE (0.375, 1.15)
        new Vector2(0.375f, 1.15f),
        // 2: N (-0.1, 1.15)
        new Vector2(-0.1f, 1.15f),
        // 3: NW (-0.53f, 1.036f)
        new Vector2(-0.53f, 1.036f),
        // 4: W (-0.585f, 0.72f)
        new Vector2(-0.585f, 0.72f),
        // 5: SW (-0.315f, 0.428f)
        new Vector2(-0.315f, 0.428f),
        // 6: S (0.195f, 0.375f)
        new Vector2(0.195f, 0.375f),
        // 7: SE (0.585f, 0.555f)
        new Vector2(0.585f, 0.555f)
    };

    public Vector2 GetShootPositionFromIndex(Vector2 attackerPosition, int directionIndex)
    {
        // 인덱스 유효성 검사 (0~7)
        if (directionIndex < 0 || directionIndex >= LOCAL_OFFSETS.Length)
        {
            Debug.LogError($"잘못된 방향 인덱스: {directionIndex}. 기본 위치를 반환합니다.");
            return attackerPosition;
        }

        Vector2 localOffset = LOCAL_OFFSETS[directionIndex];

        // Root 위치 (attackerPosition)에 로컬 오프셋을 더하여 월드 위치를 계산합니다.
        return attackerPosition + localOffset;
    }
}
