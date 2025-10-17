using UnityEngine;

// 이 클래스는 실제 게임 빌드에서는 제외되거나, Debug.isDebugBuild 플래그로 보호되어야 합니다.
public class AreaDebugDrawer
{
    // 디버그 드로잉 지속 시간
    private const float DEBUG_DRAW_DURATION = 1.0f;

    public void DrawArea(Vector2 center, AreaShapeType shape, float radius, Vector2 size, float angle, Vector2 direction)
    {
        // Debug 모드가 아니면 실행하지 않음
        if (!Debug.isDebugBuild) return;

        Color color = Color.red;

        switch (shape)
        {
            case AreaShapeType.Circle:
                DrawCircle(center, radius, color);
                break;
            case AreaShapeType.Box:
                DrawBox(center, size, angle, color);
                break;
            case AreaShapeType.Cone:
                DrawCone(center, radius, angle, direction, color);
                break;
            default:
                break;
        }
    }

    // 간단한 원형 디버그 드로잉 (Gizmos 또는 Debug.DrawRay 사용)
    private void DrawCircle(Vector2 center, float radius, Color color)
    {
        // 유니티 에디터 환경에서 디버깅하는 Gizmos 로직이 포함됩니다.
        // Runtime에서는 Debug.DrawRay를 반복해서 원을 근사하게 그릴 수 있습니다.
        Debug.DrawRay(center, Vector3.up * 0.1f, Color.yellow, DEBUG_DRAW_DURATION); // 중심 표시
    }

    // 사각형 디버그 드로잉
    private void DrawBox(Vector2 center, Vector2 size, float angle, Color color)
    {
        // 유니티 에디터의 Debug.DrawRay 또는 Debug.DrawLine을 사용하여 사각형의 네 변을 그립니다.
        // Quaternion.Euler(0, 0, angle)을 사용하여 회전 변환을 적용합니다.
        Debug.DrawRay(center, Vector3.right * size.x / 2, Color.cyan, DEBUG_DRAW_DURATION); // 예시
    }

    // 원뿔/부채꼴 디버그 드로잉
    private void DrawCone(Vector2 center, float radius, float angle, Vector2 direction, Color color)
    {
        // Cone의 중심 선과 양쪽 경계선을 그립니다.
        // MathF.Cos, MathF.Sin을 사용하여 경계 각도를 계산하고 Debug.DrawRay를 사용합니다.
        Debug.DrawRay(center, direction * radius, Color.magenta, DEBUG_DRAW_DURATION); // 중심 방향
    }
}