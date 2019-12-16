using UnityEngine;
using Urbanice.Module.Containers;

namespace Urbanice
{
    /// <summary>
    /// This simple class provides direct access to the <see cref="CityDefinitionContainer"/> to generate a new city
    /// from the inspector
    /// </summary>
    [ExecuteInEditMode]
    public class CityGeneratorScript : MonoBehaviour
    {

        public CityDefinitionContainer CityDefinitionContainer;
    
        private void OnValidate()
        {
            GenerateCity();
        }
    
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                GenerateCity();
            }
        }

        public void GenerateCity()
        {
            CityDefinitionContainer.GenerateCity();
        }
    }
}
