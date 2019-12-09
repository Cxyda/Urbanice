using UnityEditor;
using UnityEngine;
using Urbanice.Module.Data;

namespace Urbanice.Module.Layers
{
    /// <summary>
    /// This class is just an example. Doesn't do anything in the current state of the prototype'
    /// </summary>
    [CreateAssetMenu(menuName = "Urbanice/DataLayers/Create new Terrain Layer", fileName = "newTerrainLayer", order = 1)]
    public class TerrainLayer : BaseLayer, IUrbaniceLayer
    {
        [EnumFlagsAttribute]
        public TerrainType TerrainType;
        
        public Texture2D Heightmap;
        public Texture2D HydroMap;

        public void Init()
        {
            // nothing to do here yet
        }

        public void Generate(BaseLayer parentLayer)
        {
            // nothing to do here yet
        }
    }
    
    /// <summary>
    /// Enum Flags attribute to use bit masks for terrain properties
    /// </summary>
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