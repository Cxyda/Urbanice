using UnityEngine;

namespace Urbanice.Editor
{
    [UnityEditor.CustomEditor(typeof(CityGeneratorScript))]
    public class GenerateScriptEditor : UnityEditor.Editor
    {
        private CityGeneratorScript _target;
        public override void OnInspectorGUI()
        {
            _target = (CityGeneratorScript)target;
            
            base.OnInspectorGUI();
            GUILayout.Space(10);
            if (GUILayout.Button("Generate City"))
            {
                _target.GenerateCity();
            }
        }
    }
}