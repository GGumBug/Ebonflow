using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector에서 읽기 전용으로 표시할 필드에 적용할 커스텀 속성입니다.
/// 이 속성을 붙인 필드는 사용자가 값을 수정할 수 없으며, 오직 값을 확인만 할 수 있습니다.
/// </summary>
public class ReadOnly : PropertyAttribute
{
    // 이 클래스는 단순히 태그 역할만 수행하므로, 내부에 별도의 구현이 필요하지 않습니다.
}

/// <summary>
/// ReadOnly 속성이 붙은 필드를 Inspector에서 읽기 전용으로 표시하기 위한 커스텀 프로퍼티 드로워입니다.
/// 이 드로워는 필드를 그릴 때 GUI.enabled를 false로 설정하여 값이 수정되지 않도록 합니다.
/// </summary>
[CustomPropertyDrawer(typeof(ReadOnly))]
public class ReadOnlyDrawer : PropertyDrawer
{
    /// <summary>
    /// Inspector에 그릴 대상 프로퍼티의 높이를 계산하여 반환합니다.
    /// 하위 프로퍼티까지 포함하여 계산합니다.
    /// </summary>
    /// <param name="property">그릴 대상 SerializedProperty</param>
    /// <param name="label">프로퍼티에 표시될 라벨</param>
    /// <returns>프로퍼티를 그리기 위해 필요한 높이</returns>
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 기본 EditorGUI.GetPropertyHeight()를 호출하여, 하위 프로퍼티를 포함한 전체 높이를 반환합니다.
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    /// <summary>
    /// ReadOnly 속성이 적용된 필드를 Inspector에 읽기 전용으로 그립니다.
    /// </summary>
    /// <param name="position">필드를 그릴 영역의 Rect</param>
    /// <param name="property">그릴 대상 SerializedProperty</param>
    /// <param name="label">프로퍼티에 표시될 라벨</param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // GUI를 비활성화하여 사용자가 값을 변경할 수 없도록 합니다.
        GUI.enabled = false;
        // 지정된 영역(position)에 프로퍼티 필드를 그립니다.
        // 마지막 인자인 true는 하위 프로퍼티들도 그리도록 합니다.
        EditorGUI.PropertyField(position, property, label, true);
        // GUI 활성 상태를 원래대로 복원합니다.
        GUI.enabled = true;
    }
}
