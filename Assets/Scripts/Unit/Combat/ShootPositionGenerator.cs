using UnityEngine;

public class ShootPositionGenerator
{
    private const float MaxX = 0.5f;
    private const float MaxY = 1.3f;

    public Vector2 GetShootPositionFromDirection(Vector2 attackerPosition, Vector2 direction)
    {
        if (direction.sqrMagnitude < Mathf.Epsilon)
        {
            return attackerPosition;
        }

        // 2. 부호(Sign)를 사용하여 X, Y 오프셋을 결정합니다.
        // Mathf.Sign() 함수는 양수일 때 1, 음수일 때 -1, 0일 때 0을 반환합니다.

        float signX = Mathf.Sign(direction.x);
        float signY = Mathf.Sign(direction.y);

        float offsetX = 0f;
        float offsetY = 0f;

        // 3. 오프셋 계산: 부호에 따라 Max 값을 적용합니다.

        // X 오프셋 결정
        if (signX != 0)
        {
            offsetX = MaxX * signX;
        }
        // Y 오프셋 결정
        if (signY != 0)
        {
            offsetY = MaxY * signY;
        }

        // 4. 특별 처리: 축 방향 이동 보장 (선택적)
        // 만약 direction이 (1, 0)처럼 순수한 축 방향일 경우, 
        // 오프셋이 (MaxX, 0)이나 (0, MaxY)가 되도록 처리할 수도 있습니다.
        // 현재 로직은 (1, 0)일 경우, signX=1, signY=0이므로 offsetX=MaxX, offsetY=0이 되어 (0.25, 0)을 반환합니다.
        // (1, 1)일 경우, signX=1, signY=1이므로 offsetX=MaxX, offsetY=MaxY가 되어 (0.25, 0.15)을 반환합니다.

        Vector2 offset = new Vector2(offsetX, offsetY);

        // 5. 최종 발사 위치 반환
        return attackerPosition + offset;
    }
}
