using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Urbanice.Module.Wards;

namespace Urbanice.Module.Districts.Editor
{
    /// <summary>
    /// This class is responsible for rendering the <see cref="WardsConfig" class in the Unity inspector/>
    /// </summary>
    [CustomEditor(typeof(WardsConfig))]
    public class WardConfigEditor : UnityEditor.Editor {
        
        private ReorderableList _weightsList;
        private ReorderableList _districtsList;
	
        private SerializedProperty _wardType;
        private SerializedProperty _renderColor;
        
        private void OnEnable() {
            
            _wardType = serializedObject.FindProperty("WardType");

            _renderColor = serializedObject.FindProperty("RenderColor");
            
            _districtsList = new ReorderableList(serializedObject, 
                serializedObject.FindProperty("DistrictTypes"), 
                true, true, true, true);
            _districtsList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Available DistrictTypes");
            };
            
            _districtsList.drawElementCallback = 
                (Rect rect, int index, bool isActive, bool isFocused) => {
                    var element = _districtsList.serializedProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, rect.width * .5f, EditorGUIUtility.singleLineHeight),
                        element, GUIContent.none);
                };
            
            
            
            _weightsList = new ReorderableList(serializedObject, 
                serializedObject.FindProperty("NeighboringWards"), 
                true, true, true, true);
            _weightsList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Neighboring Ward Weights");
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
            
            EditorGUILayout.PropertyField(_wardType);
            EditorGUILayout.PropertyField(_renderColor);
            
            _districtsList.DoLayoutList();

            _weightsList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}