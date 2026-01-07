using UnityEngine;
using UnityEditor;

namespace MatchBattle
{
    /// <summary>
    /// RelicData 커스텀 Inspector
    /// </summary>
    [CustomEditor(typeof(RelicData))]
    public class RelicDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // 기본 Inspector 그리기
            DrawDefaultInspector();

            EditorGUILayout.Space();

            RelicData relicData = (RelicData)target;

            // "Generate Description" 버튼
            if (GUILayout.Button("Generate Description", GUILayout.Height(30)))
            {
                SerializedObject so = new SerializedObject(relicData);
                SerializedProperty descProp = so.FindProperty("description");

                string generatedDesc = relicData.GenerateDescription();
                descProp.stringValue = generatedDesc;
                so.ApplyModifiedProperties();
            }

            // "Clear Description" 버튼 (테스트용)
            if (GUILayout.Button("Clear Description"))
            {
                SerializedObject so = new SerializedObject(relicData);
                SerializedProperty descProp = so.FindProperty("description");
                descProp.stringValue = "";
                so.ApplyModifiedProperties();
            }
        }
    }
}
