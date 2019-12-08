namespace Urbanice.Module.Layers
{
    public interface IUrbaniceLayer
    {
        void Init();
        void Generate(BaseLayer parentLayer);
    }
}