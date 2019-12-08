using UnityEngine;

namespace Urbanice.Module.Containers.Editor
{
    [UnityEditor.CustomEditor(typeof(CityDefinitionContainer))]
    public class CityDefinitionContainerEditor: UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            CityDefinitionContainer myTarget = (CityDefinitionContainer)target;
            DrawDefaultInspector();

            if (GUILayout.Button("Generate City"))
            {
                myTarget.GenerateCity();
            }
        }
    }
}