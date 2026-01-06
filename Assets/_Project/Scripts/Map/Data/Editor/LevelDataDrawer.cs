using UnityEngine;
using UnityEditor;

namespace MatchBattle
{
    [CustomPropertyDrawer(typeof(LevelData))]
    public class LevelDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 리스트 인덱스에서 레벨 번호 추출 (Element 0 → Level 1)
            string labelText = label.text;
            if (labelText.StartsWith("Element "))
            {
                int index = int.Parse(labelText.Substring(8));
                label.text = $"Level {index + 1}";
            }

            // Level 폴드아웃 그리기
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                SerializedProperty stagesProp = property.FindPropertyRelative("stages");
                float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // stages 리스트의 각 항목을 직접 그림 ("Stages" 레이블 건너뛰기)
                if (stagesProp != null && stagesProp.isArray)
                {
                    for (int i = 0; i < stagesProp.arraySize; i++)
                    {
                        SerializedProperty stageProp = stagesProp.GetArrayElementAtIndex(i);
                        float stageHeight = EditorGUI.GetPropertyHeight(stageProp, true);
                        Rect stageRect = new Rect(position.x, position.y + yOffset, position.width, stageHeight);

                        EditorGUI.PropertyField(stageRect, stageProp, true);

                        yOffset += stageHeight + EditorGUIUtility.standardVerticalSpacing;
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
                SerializedProperty stagesProp = property.FindPropertyRelative("stages");

                if (stagesProp != null && stagesProp.isArray)
                {
                    for (int i = 0; i < stagesProp.arraySize; i++)
                    {
                        SerializedProperty stageProp = stagesProp.GetArrayElementAtIndex(i);
                        height += EditorGUI.GetPropertyHeight(stageProp, true) + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }

            return height;
        }
    }
}
