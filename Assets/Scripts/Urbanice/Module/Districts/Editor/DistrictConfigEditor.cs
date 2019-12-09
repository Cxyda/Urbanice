using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Urbanice.Module.Districts.Editor
{
    /// <summary>
    /// This class is responsible for rendering the <see cref="DistrictConfig" class in the Unity inspector/>
    /// </summary>
    [CustomEditor(typeof(DistrictConfig))]
    public class DistrictConfigEditor : UnityEditor.Editor {
        
        private ReorderableList _weightsList;
	
        private SerializedProperty _districtType;
        private SerializedProperty _renderColor;
        private SerializedProperty _initialSpawnType;
        private SerializedProperty _patternGenerator;      
        
        private void OnEnable() {
            
            _districtType = serializedObject.FindProperty("DistrictType");
            _renderColor = serializedObject.FindProperty("RenderColor");
            _initialSpawnType = serializedObject.FindProperty("InitialSpawnType");
            _patternGenerator = serializedObject.FindProperty("PatternGenerator");
            
            _weightsList = new ReorderableList(serializedObject, 
                serializedObject.FindProperty("NeighboringDistricts"), 
                true, true, true, true);
            _weightsList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Neighboring District Weights");
            };
            
            _weightsList.drawElementCallback = 
                (Rect rect, int index, bool isActive, bool isFocused) => {
                    var element = _weightsList.serializedProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, rect.width * .5f, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("Element"), GUIContent.none);
                    EditorGUI.LabelField(new Rect(rect.x + rect.width * .5f + 5, rect.y, 55, EditorGUIUtility.singleLineHeight), 
                        new GUIContent("Weight:"));
                    EditorGUI.PropertyField(
                        new Rect(rect.x + rect.width *.5f + 60, rect.y, rect.width *.5f - 60, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("Weight"), GUIContent.none);
                };
        }
	
        public override void OnInspectorGUI() {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(_districtType);
            EditorGUILayout.PropertyField(_renderColor);
            EditorGUILayout.PropertyField(_initialSpawnType);
            EditorGUILayout.PropertyField(_patternGenerator);
            
            _weightsList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}