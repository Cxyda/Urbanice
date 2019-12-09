using UnityEngine;
using Urbanice.Module.Containers;

namespace Urbanice
{
    [ExecuteInEditMode]
    public class CityGeneratorScript : MonoBehaviour
    {

        public CityDefinitionContainer CityDefinitionContainer;
    
        private void OnValidate()
        {
            //GenerateCity();
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
