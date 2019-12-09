namespace Urbanice.Module.Layers
{
    /// <summary>
    /// The basic urbanice layer interface
    /// </summary>
    public interface IUrbaniceLayer
    {
        void Init();
        void Generate(BaseLayer parentLayer);
    }
}