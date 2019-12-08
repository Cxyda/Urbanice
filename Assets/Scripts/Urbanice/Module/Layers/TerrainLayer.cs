using UnityEditor;
using UnityEngine;
using Urbanice.Module.Data;

namespace Urbanice.Module.Layers
{
    [CreateAssetMenu(menuName = "Urbanice/DataLayers/Create new Terrain Layer", fileName = "newTerrainLayer", order = 1)]
    public class TerrainLayer : BaseLayer, IUrbaniceLayer
    {
        [EnumFlagsAttribute]
        public TerrainType TerrainType;
        
        public Texture2D Heightmap;
        public Texture2D HydroMap;

        public void Init()
        {
            
        }

        public void Generate(BaseLayer parentLayer)
        {
            // TODO
        }
    }
    public class EnumFlagsAttribute : PropertyAttribute
    {
        public EnumFlagsAttribute() { }
    }
 
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            _property.intValue = EditorGUI.MaskField( _position, _label, _property.intValue, _property.enumNames );
        }
    }
}