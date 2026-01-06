using UnityEngine;
using UnityEditor;

namespace MatchBattle
{
    [CustomPropertyDrawer(typeof(StageGroup))]
    public class StageGroupDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // choices 가져오기
            SerializedProperty choicesProp = property.FindPropertyRelative("choices");

            // 리스트 인덱스에서 스테이지 번호 추출 (Element 0 → Stage 1)
            string labelText = label.text;
            if (labelText.StartsWith("Element "))
            {
                int stageIndex = int.Parse(labelText.Substring(8)) + 1;

                // 보스 스테이지인지 확인
                bool isBoss = false;
                if (choicesProp != null && choicesProp.arraySize > 0)
                {
                    SerializedProperty firstChoice = choicesProp.GetArrayElementAtIndex(0);
                    SerializedProperty stageTypeProp = firstChoice.FindPropertyRelative("stageType");
                    if (stageTypeProp != null && stageTypeProp.enumValueIndex == (int)StageType.Boss)
                    {
                        isBoss = true;
                    }
                }

                label.text = isBoss ? $"보스 스테이지" : $"Stage {stageIndex} ({choicesProp?.arraySize ?? 0}개 선택지)";
            }

            // Stage 폴드아웃 그리기
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // choices 리스트의 각 항목을 직접 그림 ("Choices" 레이블 건너뛰기)
                if (choicesProp != null && choicesProp.isArray)
                {
                    for (int i = 0; i < choicesProp.arraySize; i++)
                    {
                        SerializedProperty choiceProp = choicesProp.GetArrayElementAtIndex(i);
                        float choiceHeight = EditorGUI.GetPropertyHeight(choiceProp, true);
                        Rect choiceRect = new Rect(position.x, position.y + yOffset, position.width, choiceHeight);

                        // "1-1", "1-2" 형식으로 레이블 생성
                        string choiceLabel = $"{i + 1}";
                        EditorGUI.PropertyField(choiceRect, choiceProp, new GUIContent(choiceLabel), true);

                        yOffset += choiceHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
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
                SerializedProperty choicesProp = property.FindPropertyRelative("choices");

                if (choicesProp != null && choicesProp.isArray)
                {
                    for (int i = 0; i < choicesProp.arraySize; i++)
                    {
                        SerializedProperty choiceProp = choicesProp.GetArrayElementAtIndex(i);
                        height += EditorGUI.GetPropertyHeight(choiceProp, true) + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }

            return height;
        }
    }
}
