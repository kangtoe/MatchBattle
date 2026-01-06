using UnityEngine;
using UnityEditor;

namespace MatchBattle
{
    [CustomPropertyDrawer(typeof(StageNode))]
    public class StageNodeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty stageTypeProp = property.FindPropertyRelative("stageType");
            SerializedProperty isCompletedProp = property.FindPropertyRelative("isCompleted");

            // 스테이지 타입 이름 가져오기
            string stageTypeName = stageTypeProp != null ? ((StageType)stageTypeProp.enumValueIndex).ToString() : "???";
            bool isCompleted = isCompletedProp != null && isCompletedProp.boolValue;

            // 한 줄로 표시: "1: Combat [✓]" 또는 "1: Combat [ ]"
            string displayText = isCompleted ? $"{label.text}: {stageTypeName} [✓]" : $"{label.text}: {stageTypeName}";

            // 폴드아웃으로 상세 정보 표시 가능
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, displayText, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // 상세 정보 표시
                SerializedProperty levelIndexProp = property.FindPropertyRelative("levelIndex");
                SerializedProperty stageIndexProp = property.FindPropertyRelative("stageIndex");

                if (levelIndexProp != null)
                {
                    Rect levelRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(levelRect, levelIndexProp);
                    yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                if (stageIndexProp != null)
                {
                    Rect stageRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(stageRect, stageIndexProp);
                    yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                if (stageTypeProp != null)
                {
                    Rect typeRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(typeRect, stageTypeProp);
                    yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                if (isCompletedProp != null)
                {
                    Rect completedRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(completedRect, isCompletedProp);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                // 4개 필드 (levelIndex, stageIndex, stageType, isCompleted)
                height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4;
            }

            return height;
        }
    }
}
